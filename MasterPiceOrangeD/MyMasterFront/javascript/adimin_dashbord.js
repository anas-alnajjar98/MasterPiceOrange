
function hideAllSections() {
    debugger
    const sections = document.querySelectorAll('.dashboard-data-section');
    sections.forEach(section => {
        section.style.display = 'none';
    });
}
////////////////////////////////
document.addEventListener('DOMContentLoaded', function () {
    async function fetchBusinessStats() {
        try {
            
            const response = await fetch('https://localhost:7260/api/Business/GetBusinessStats');
            
            // Handle non-OK responses
            if (!response.ok) {
                throw new Error(`Failed to fetch business stats: ${response.status} ${response.statusText}`);
            }
            
            // Parse the JSON response
            const data = await response.json();
    
            // Display the stats on the page
            document.getElementById("total-users").textContent = data.totaluser || 0;
            document.getElementById("total-products").textContent = data.totalproducts || 0;
            document.getElementById("total-bids").textContent = data.totalBids || 0;
        } catch (error) {
            console.error("Error fetching business stats:", error);
    
            // Handle errors gracefully (e.g., show an alert or placeholder content)
            document.getElementById("business-stats-error").textContent = "Failed to load business stats.";
        }
    }
    
    async function getUserInfo() {
        const userId = localStorage.getItem("userId");
        
        if (!userId) {
            console.error('User ID not found in localStorage');
            return;
        }

        try {
            const response = await fetch(`https://localhost:7260/api/User/GetUserByID/${userId}`);
            if (!response.ok) {
                throw new Error('Failed to fetch user info');
            }
            const data = await response.json();
            console.log(data);
            //document.getElementById("userImg").src =`https://localhost:7046/${data.imageUrl}`

            document.getElementById("userImg").src = data.imageUrl.startsWith("/images/")
                ? `https://localhost:7260/${data.imageUrl}`
                : data.imageUrl;
            
            document.getElementById("userName").innerText = data.username;
            document.getElementById("userEmail").innerText = data.email;

            document.getElementById("usernameInput").value = data.username;
            document.getElementById("emailInput").value = data.email;
            document.getElementById("imageInput").value = data.imageUrl;
        } catch (e) {
            console.error('Error fetching user info:', e);
        }
    }
    // Dashboard
    document.getElementById('dashboard-link').addEventListener('click', async function () {
        hideAllSections();
        document.getElementById('dashboard-section').style.display = 'block';
        try {
            const response = await fetch('https://yourapi.com/dashboard');
            const data = await response.json();
            document.getElementById('active-bids-count').textContent = data.activeBids;
            document.getElementById('items-won-count').textContent = data.itemsWon;
            document.getElementById('favorites-count').textContent = data.favorites;
        } catch (error) {
            console.error('Error fetching dashboard data', error);
        }
    });
async function messegeToWinners() {
        try {
            const response = await fetch(`https://localhost:7260/api/Admin/EndedAuction`);
            const data = await response.json();

            
            const tableBody = document.querySelector("#winners-table tbody");
            tableBody.innerHTML = "";  
            document.getElementById("messages-section").style.display = "block";

            if (!data.endedAuctions || data.endedAuctions.length === 0) {
                tableBody.innerHTML = '<tr><td colspan="6">No auctions have ended.</td></tr>';
                return;
            }

            data.endedAuctions.forEach(auction => {
                const row = document.createElement("tr");

                // Create table cells
                const auctionIdCell = document.createElement("td");
                auctionIdCell.textContent = auction.AuctionId;

                const productNameCell = document.createElement("td");
                productNameCell.textContent = auction.ProductName;

                const highestBidCell = document.createElement("td");
                highestBidCell.textContent = `$${auction.CurrentHighestBid.toFixed(2)}`;

                const highestBidderCell = document.createElement("td");
                highestBidderCell.textContent = auction.HighestBidder ? auction.HighestBidder.Username : "No Bidder";

                const bidderEmailCell = document.createElement("td");
                bidderEmailCell.textContent = auction.HighestBidder ? auction.HighestBidder.Email : "N/A";

                // Create action button for sending email
                const actionCell = document.createElement("td");
                const emailButton = document.createElement("button");
                emailButton.textContent = "Send Email";
                emailButton.classList.add("btn", "btn-primary");
                emailButton.onclick = function () {
                    sendEmail(auction.HighestBidder.Email, auction.ProductName, auction.AuctionId);
                };
                actionCell.appendChild(emailButton);

                // Append cells to row
                row.appendChild(auctionIdCell);
                row.appendChild(productNameCell);
                row.appendChild(highestBidCell);
                row.appendChild(highestBidderCell);
                row.appendChild(bidderEmailCell);
                row.appendChild(actionCell);

                // Append row to table body
                tableBody.appendChild(row);
            });

            // Display the table section
            document.getElementById("messages-section").style.display = "block";
        } catch (error) {
            console.error("Error fetching or displaying auction data:", error);
        }
    }
////////////////////////////////////////////   
    // Load Pending Products
    async function loadPendingProducts() {
        const container = document.querySelector('#product-container');
        container.innerHTML = ''; // Clear the container
    
        try {
            const response = await fetch('https://localhost:7260/api/Admin/GetAllProductToPlaceAuction');
            const data = await response.json();
    
            if (data.length === 0) {
                container.innerHTML = '<p>No pending products found.</p>';
                return;
            }
    
            const row = document.createElement('div');
            row.classList.add('row'); // Use a row container for cards
    
            data.forEach(product => {
                const card = document.createElement('div');
                card.classList.add('col-md-6', 'col-lg-6', 'mb-4'); // Two cards per row, responsive classes
                
                card.innerHTML = `
                    <div class="card">
                        <img src="https://localhost:7260${product.imageUrl}" class="card-img-top" alt="${product.productName}">
                        <div class="card-body">
                            <h5 class="card-title">${product.productName}</h5>
                            <p class="card-text">${product.description}</p>
                            <p><strong>Price:</strong> $${product.startingPrice.toFixed(2)}</p>
                            <p><strong>Stock:</strong> ${product.stock}</p>
                            <p><strong>Condition:</strong> ${product.condition}</p>
                            <p><strong>Location:</strong> ${product.location}</p>
                            <p><strong>Brand:</strong> ${product.brand}</p>
                            <p><strong>Category:</strong> ${product.categoryName}</p>
                            <div class="actions d-flex justify-content-between">
<button class="btn btn-success btn-accept" data-id="${product.productId}" data-price="${product.startingPrice}">Accept</button>
                                <button class="btn btn-danger btn-reject" data-id="${product.productId}">Reject</button>
                            </div>
                        </div>
                    </div>
                `;
                
                row.appendChild(card);
            });
    
            container.appendChild(row);
    
            document.querySelectorAll('.btn-accept').forEach(button => {
                debugger
                button.addEventListener('click', function () {
                    const productId = this.getAttribute('data-id');
                    const startingPrice = this.getAttribute('data-price');
                    acceptProduct(productId, startingPrice);
                });
            });
    
            document.querySelectorAll('.btn-reject').forEach(button => {
                button.addEventListener('click', function () {
                    const productId = this.getAttribute('data-id');
                    rejectProduct(productId);
                });
            });
    
        } catch (error) {
            console.error('Error fetching pending products', error);
        }
    }
    
    
    
    
/////////////////////////////////////////////
    // Function to accept a product
    async function acceptProduct(productId, startingPrice) {
        // Populate the modal fields with product data
        document.getElementById('productIdInput').value = productId;
        document.getElementById('startingPriceInput').value = startingPrice;
    
        var auctionModal = new bootstrap.Modal(document.getElementById('auctionModal'));
        auctionModal.show();

    }
   
/////////////////////////////////////////////
    // Function to reject a product
    async function rejectProduct(productId) {
        Swal.fire({
            title: 'Are you sure?',
            text: 'You are about to reject this product. This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, reject it!'
        }).then(async (result) => {
            if (result.isConfirmed) {
                try {
                    const response = await fetch(`https://localhost:7260/api/Admin/RejectProduct/${productId}`, {
                        method: 'PUT'
                    });
    
                    if (response.ok) {
                        Swal.fire(
                            'Rejected!',
                            'The product has been successfully rejected.',
                            'success'
                        );
                        loadPendingProducts(); // Reload the list of pending products
                    } else {
                        Swal.fire(
                            'Failed!',
                            'Failed to reject the product. Please try again.',
                            'error'
                        );
                    }
                } catch (error) {
                    console.error('Error rejecting product:', error);
                    Swal.fire(
                        'Error!',
                        'An unexpected error occurred. Please try again later.',
                        'error'
                    );
                }
            }
        });
    }
    
/////////////////////////////////////////////
    // Load Pending Products when the 'Active Bids' section is clicked
    document.getElementById('active-bids-link').addEventListener('click', async function () {
        debugger
       hideAllSections();
        document.getElementById('active-bids-section').style.display = 'block';
        loadPendingProducts();
    });
/////////////////////////////////////////////
    document.getElementById('user-info-link').addEventListener('click', async function () {
        hideAllSections();
        document.getElementById('user-info-section').style.display = 'block';

        try {
            const response = await fetch('https://localhost:7260/api/Admin/GetAllUsers');
            const data = await response.json();
            const tableBody = document.querySelector('#user-info-table tbody');
            tableBody.innerHTML = '';
            data.forEach(user => {
                const row = document.createElement('tr');

                row.innerHTML = `
                    <td>${user.userId}</td>
                    <td>${user.username}</td>
                    <td>${user.email}</td>
                    <td>${user.address || 'N/A'}</td>
                    <td>${user.gender}</td>
                    <td>
                        <button class="btn btn-primary btn-edit" data-id="${user.userId}">Edit</button>
                        <button class="btn btn-danger btn-delete" data-id="${user.userId}">Delete</button>
                    </td>
                `;

                tableBody.appendChild(row);
            });

            // Add event listeners for the Edit and Delete buttons
            document.querySelectorAll('.btn-edit').forEach(button => {
                button.addEventListener('click', function () {
                    const userId = this.getAttribute('data-id');
                    editUser(userId); // Call the edit function
                });
            });

            document.querySelectorAll('.btn-delete').forEach(button => {
                button.addEventListener('click', function () {
                    const userId = this.getAttribute('data-id');
                    deleteUser(userId); // Call the delete function
                });
            });

        } catch (error) {
            console.error('Error fetching user info', error);
        }
    });
/////////////////////////////////////////////
    // Edit User
    async function editUser(userId) {
        try {
            const response = await fetch(`https://localhost:7260/api/User/GetUserByID/${userId}`);
            const userData = await response.json();
            
            document.getElementById('editUsername').value = userData.username;
            document.getElementById('editEmail').value = userData.email;
            document.getElementById('editAddress').value = userData.address || '';
            document.getElementById('editGender').value = userData.gender;
            
            var editUserModal = new bootstrap.Modal(document.getElementById('editUserModal'));
            editUserModal.show();

            // Save the changes
            document.getElementById('saveUserChanges').addEventListener('click', async function () {
                const formData = new FormData();
                formData.append('Username', document.getElementById('editUsername').value);
                formData.append('Email', document.getElementById('editEmail').value);
                formData.append('Address', document.getElementById('editAddress').value);
                formData.append('Gender', document.getElementById('editGender').value);

                const imageFile = document.getElementById('editImage').files[0];
                if (imageFile) {
                    formData.append('Image', imageFile);
                }

                try {
                    const response = await fetch(`https://localhost:7260/api/User/UpdateUserInfoWithImage/${userId}`, {
                        method: 'POST',
                        body: formData
                    });

                    if (response.ok) {
                        alert('User updated successfully');
                        editUserModal.hide();
                        document.getElementById('user-info-link').click();
                    } else {
                        alert('Failed to update user');
                    }
                } catch (error) {
                    console.error('Error updating user:', error);
                }
            });
        } catch (error) {
            console.error('Error fetching user info:', error);
        }
    }
////////////////////////////////////////////
    // Delete User
    async function deleteUser(userId) {
        Swal.fire({
            title: 'Are you sure?',
            text: 'You are about to delete this user. This action cannot be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then(async (result) => {
            if (result.isConfirmed) {
                try {
                    const response = await fetch(`https://localhost:7260/api/Admin/DeleteUser/${userId}`, {
                        method: 'DELETE'
                    });
    
                    if (response.ok) {
                        Swal.fire(
                            'Deleted!',
                            'The user has been successfully deleted.',
                            'success'
                        );
                        document.getElementById('user-info-link').click();
                    } else {
                        Swal.fire(
                            'Failed!',
                            'An error occurred while trying to delete the user. Please try again.',
                            'error'
                        );
                    }
                } catch (error) {
                    console.error('Error deleting user:', error);
                    Swal.fire(
                        'Error!',
                        'An unexpected error occurred. Please try again later.',
                        'error'
                    );
                }
            }
        });
    }
///////////////////////////////////////////    
    async function messegeToWinners() {
        try {
            debugger
            hideAllSections();
            const response = await fetch(`https://localhost:7260/api/Admin/EndedAuction`);
            const data = await response.json();
console.log(data)
document.getElementById("messages-section").style.display = "block";

            const tableBody = document.querySelector("#winners-table tbody");
            tableBody.innerHTML = "";  

            if (!data.endedAuctions || data.endedAuctions.length === 0) {
                tableBody.innerHTML = '<tr><td colspan="6">No auctions have ended.</td></tr>';
                return;
            }

            data.endedAuctions.forEach(auction => {
                const row = document.createElement("tr");

                const auctionIdCell = document.createElement("td");
                auctionIdCell.textContent = auction.auctionId;

                const productNameCell = document.createElement("td");
                productNameCell.textContent = auction.productName;

                const highestBidCell = document.createElement("td");
                highestBidCell.textContent = `$${auction.currentHighestBid.toFixed(2)}`;

                const highestBidderCell = document.createElement("td");
                highestBidderCell.textContent = auction.HighestBidder ? auction.HighestBidder.Username : "No Bidder";

                const bidderEmailCell = document.createElement("td");
                bidderEmailCell.textContent = auction.HighestBidder ? auction.HighestBidder.Email : "N/A";

                const actionCell = document.createElement("td");
                const emailButton = document.createElement("button");
                emailButton.textContent = "Send Email";
                emailButton.classList.add("btn", "btn-primary");
                emailButton.onclick = function () {
                    sendEmail( auction.auctionId);
                };
                actionCell.appendChild(emailButton);

                row.appendChild(auctionIdCell);
                row.appendChild(productNameCell);
                row.appendChild(highestBidCell);
                row.appendChild(highestBidderCell);
                row.appendChild(bidderEmailCell);
                row.appendChild(actionCell);

                tableBody.appendChild(row);
            });

            document.getElementById("messages-section").style.display = "block";
            loadPendingProducts();
        } catch (error) {
            console.error("Error fetching or displaying auction data:", error);
        }
    }
////////////////////////////
async function showblogs(){
    hideAllSections();
    document.getElementById("add-blog-section").style.display = "block";
}
document.getElementById("AddblogForm").addEventListener("submit", async function (event) {
    event.preventDefault(); // Prevent default form submission behavior
debugger
    const formData = new FormData(document.getElementById("AddblogForm"));
    formData.append("AuthorId", localStorage.getItem("userId")); // Add AuthorId to the form data from localStorage

    try {
        const response = await fetch(`https://localhost:7260/api/Admin/CreateBlog`, {
            method: "POST",
            body: formData,
        });

        if (response.ok) {
            const result = await response.json();

            // Display success message
            Swal.fire({
                icon: "success",
                title: "Blog Created",
                text: `Your blog was created successfully with ID: ${result.blogId}`,
                confirmButtonText: "OK",
            });

            // Reset the form fields
            document.getElementById("AddblogForm").reset();
        } else {
            // Handle server errors
            const errorData = await response.json();
            Swal.fire({
                icon: "error",
                title: "Error",
                text: `Failed to create the blog. ${errorData.message}`,
                confirmButtonText: "Try Again",
            });
        }
    } catch (e) {
        // Handle network or unexpected errors
        Swal.fire({
            icon: "error",
            title: "Error",
            text: "An unexpected error occurred. Please try again later.",
            confirmButtonText: "OK",
        });
        console.error("Error creating blog:", e);
    }
});

    // Set default section
    document.getElementById('dashboard-link').click();
    document.getElementById('winners-link').addEventListener('click', messegeToWinners);
    document.getElementById('add-blog-link').addEventListener('click',  showblogs);
    getUserInfo();
    fetchBusinessStats();
});
document.getElementById('auctionForm').addEventListener('submit', async function(event) {
    event.preventDefault(); // Prevent the form from submitting in the traditional way

    const productId = document.getElementById('productIdInput').value;
    const startingPrice = document.getElementById('startingPriceInput').value;
    const duration = document.getElementById('durationInput').value;
    const durationInMinutes = document.getElementById('durationInputMinute').value

    const auctionDto = {
        ProductId: parseInt(productId),
        StartingPrice: parseFloat(startingPrice),
        DurationHours: parseInt(duration),
        durationMinutes: parseInt(durationInMinutes)
    };

    try {
        const response = await fetch(`https://localhost:7260/api/Auction/CreateAuction`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(auctionDto)
        });

        if (response.ok) {
            alert('Auction created successfully.');
            var auctionModal = bootstrap.Modal.getInstance(document.getElementById('auctionModal'));
            auctionModal.hide(); 
            loadPendingProducts(); 
        } else {
            alert('Failed to create the auction.');
        }
    } catch (error) {
        console.error('Error creating auction:', error);
    }
});
async function sendEmail ( id){
    debugger
    const response= await fetch(`https://localhost:7046/api/Auction/EndAuction/${id}`,{
method:'POST',
    });
    if(response.ok){
        alert('Auction ended successfully.');
        messegeToWinners();
    }
    else{
        alert('Failed to end the auction.');
    }
}


document.getElementById("Logout").addEventListener('click', function () {
    localStorage.removeItem('userId'); // Remove user ID
    localStorage.removeItem('token'); // Remove token
    localStorage.removeItem('user');
    localStorage.removeItem('username')
    localStorage.removeItem('isAdmin'); //
    localStorage.removeItem('email'); //
    window.location.href = 'login.html'; // Redirect to login page
});