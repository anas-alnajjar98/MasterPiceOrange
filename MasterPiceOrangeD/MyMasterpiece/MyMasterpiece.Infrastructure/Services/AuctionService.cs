using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyMasterpiece.Application.DTOs.AuctionDto;
using MyMasterpiece.Application.Interfaces;
using MyMasterpiece.Domain.Entities;
using MyMasterpiece.Infrastructure.Data;
using MyMasterpiece.Infrastructure.Services;

public class AuctionService : IAuctionService
{
    private readonly AuctionDbContext _context;
    private readonly EmailHelper _emailHelper;
    private readonly ILogger<AuctionService> _logger;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public AuctionService(AuctionDbContext context, EmailHelper emailHelper, ILogger<AuctionService> logger, IBackgroundJobClient backgroundJobClient)
    {
        _context = context;
        _emailHelper = emailHelper;
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<int> CreateAuctionAsync(CreateAuctionDto auctionDto)
    {
        _logger.LogInformation("Creating auction for product ID {ProductId}", auctionDto.ProductId);

        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == auctionDto.ProductId);

        if (product == null)
        {
            _logger.LogWarning("Product with ID {ProductId} not found.", auctionDto.ProductId);
            throw new InvalidOperationException("Product not found.");
        }

        if (product.ApprovalStatus != "Pending")
        {
            _logger.LogWarning("Product with ID {ProductId} is not in a pending state.", auctionDto.ProductId);
            throw new InvalidOperationException("Product is not in a pending state.");
        }

        var auction = new Auction
        {
            ProductId = auctionDto.ProductId,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(auctionDto.DurationHours).AddMinutes(auctionDto.DurationMinutes),
            CurrentHighestBid = product.StartingPrice,
            AuctionStatus = "ongoing"
        };

        _context.Auctions.Add(auction);
        product.ApprovalStatus = "Accepted";
        _context.Products.Update(product);

        await _context.SaveChangesAsync();

        
        _backgroundJobClient.Schedule(() => EndAuctionAsync(auction.AuctionId), auction.EndTime);

        _logger.LogInformation("Auction created successfully with ID {AuctionId}", auction.AuctionId);

        return auction.AuctionId;
    }

    public async Task EndAuctionAsync(int auctionId)
    {
        _logger.LogInformation("Ending auction with ID {AuctionId}", auctionId);

        var auction = await _context.Auctions
            .Include(a => a.Bids)
            .Include(a => a.Product)
            .FirstOrDefaultAsync(a => a.AuctionId == auctionId);

        if (auction == null)
        {
            _logger.LogWarning("Auction with ID {AuctionId} not found.", auctionId);
            return;
        }

        var highestBid = auction.Bids?.OrderByDescending(b => b.BidAmount).FirstOrDefault();
        if (highestBid == null)
        {
            auction.AuctionStatus = "failed";
            _context.Auctions.Update(auction);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Auction with ID {AuctionId} ended with no bids.", auctionId);
            return;
        }

        auction.CurrentHighestBidderId = highestBid.UserId;
        auction.CurrentHighestBid = highestBid.BidAmount;
        auction.AuctionStatus = "pending_payment";
        auction.EndTime = DateTime.Now;

        _context.Auctions.Update(auction);


        var payment = new Payment
        {
            AuctionId = auction.AuctionId,
            UserId = highestBid.UserId,
            PaymentAmount = highestBid.BidAmount,
            PaymentStatus = "pending",
            PaymentDate = DateTime.Now,
            PaymentDueDate = DateTime.Now.AddHours(24)
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();


        var paymentLink = $"http://127.0.0.1:5500/payment.html?auctionId={payment.PaymentId}";


        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == highestBid.UserId);
        if (user != null)
        {
            try
            {
                _emailHelper.SendMessage(
                    user.Username,
                    user.Email,
                    "Auction Won: Payment Required",
                    $"You have won the auction for {auction.Product.ProductName}. Please complete your payment within 24 hours by clicking the following link: {paymentLink}"
                );

                var notification = new Notification
                {
                    UserId = user.UserId,
                    Message = $"You have won the auction for {auction.Product.ProductName}. Please complete your payment within 24 hours by clicking the following link: <a href='{paymentLink}'>Payment Link</a>",
                    CreatedAt = DateTime.Now,
                    ProductId = auction.Product.ProductId,
                    AuctionId = auction.AuctionId,
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Notification and email sent successfully to user ID {UserId}", user.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email or creating notification for user ID {UserId}", user.UserId);
            }
        }

    }
    public async Task<List<MyMasterpiece.Application.DTOs.AuctionDto.PendingPaymentDto>> CheckPendingPaymentsAsync()
    {
        _logger.LogInformation("Checking for pending payments.");

        var pendingPayments = await _context.Payments
            .Include(p => p.Auction)
            .ThenInclude(a => a.Product)
            .Where(p => p.PaymentStatus == "pending" && DateTime.Now > p.PaymentDueDate)
            .ToListAsync();

        var rescheduledPayments = new List<PendingPaymentDto>();

        foreach (var payment in pendingPayments)
        {
            var auction = payment.Auction;

            if (auction != null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == auction.CurrentHighestBidderId);

                if (user != null)
                {
                    try
                    {
                        var newEndTime = DateTime.Now.AddDays(1);

                        var emailSubject = "Auction Rescheduled";

                        var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f9f9f9;
            color: #333;
            line-height: 1.6;
            padding: 20px;
        }}
        .email-container {{
            max-width: 600px;
            margin: auto;
            background-color: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }}
        h2 {{
            color: #007BFF;
            text-align: center;
        }}
        .highlight {{
            color: #28a745;
            font-weight: bold;
        }}
        .footer {{
            margin-top: 20px;
            text-align: center;
            font-size: 12px;
            color: #888;
            border-top: 1px solid #ddd;
            padding-top: 10px;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <h2>Auction Rescheduled</h2>
        <p>Dear <strong>{user.Username}</strong>,</p>
        <p>
            Unfortunately, the auction for <span class='highlight'>{auction.Product.ProductName}</span> has been rescheduled due to non-payment.
        </p>
        <p>
            The auction will now end on: <span class='highlight'>{newEndTime:dddd, MMMM d, yyyy h:mm tt}</span>.
        </p>
        <p>
            If you have any questions or need further assistance, please feel free to contact our support team.
        </p>
        <p>Best regards,<br><strong>Support Team</strong></p>
        <div class='footer'>
            &copy; {DateTime.Now.Year} Laqta team. All rights reserved.
        </div>
    </div>
</body>
</html>";

                        _emailHelper.SendMessage(user.Username, user.Email, emailSubject, emailBody);


                        _logger.LogInformation("Email sent to user {UserId} for rescheduled auction {AuctionId}.", user.UserId, auction.AuctionId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email to user {UserId} for auction {AuctionId}.", user.UserId, auction.AuctionId);
                    }
                }

                auction.CurrentHighestBid = 0;
                auction.CurrentHighestBidderId = null;
                auction.AuctionStatus = "rescheduled";
                auction.EndTime = DateTime.Now.AddDays(1);

                _context.Auctions.Update(auction);

                payment.PaymentStatus = "expired";
                _context.Payments.Update(payment);

                rescheduledPayments.Add(new PendingPaymentDto
                {
                    PaymentId = payment.PaymentId,
                    AuctionId = auction.AuctionId,
                    ProductName = auction.Product.ProductName,
                    PaymentStatus = payment.PaymentStatus,
                    PaymentDueDate = payment.PaymentDueDate,
                    Username = user?.Username,
                    UserEmail = user?.Email,
                    RescheduledEndTime = auction.EndTime
                });
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Pending payments checked. {Count} auctions rescheduled.", rescheduledPayments.Count);

        return rescheduledPayments;
    }
}
