function getQueryParam(param) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(param);
}

document.addEventListener("DOMContentLoaded", function () {
    let auctionProducts = []; 
    async function getAllCategoryWithCount() {
        const categoriesElement = document.getElementById("CategorywithCount");

        if (!categoriesElement) {
            console.error("CategorywithCount element not found in the DOM.");
            return;
        }

        try {
            const response = await fetch('https://localhost:7260/api/Products/GetALLCategoriesWithTotalProducts');
            if (!response.ok) {
                throw new Error('Failed to fetch categories');
            }

            const data = await response.json();
            categoriesElement.innerHTML = '';

            if (!data.length) {
                categoriesElement.innerHTML = '<li>No categories available</li>';
                return;
            }

            data.forEach(category => {
                categoriesElement.innerHTML += `
                    <li>
                        <a href="browse-bid.html?CategoryId=${category.categoryId}">
                            <span>${category.categoryName}</span> 
                            <span>(${category.totalProducts})</span>
                        </a>
                    </li>
                `;
            });
        } catch (error) {
            console.error('Error fetching categories:', error);
            categoriesElement.innerHTML = '<li>Error loading categories</li>';
        }
    }

    async function fetchProducts(page = 1) {
        try {
           
            let response;
            const categoryId = getQueryParam("CategoryId"); // Get categoryId from query params

            if (categoryId) {
                response = await fetch(

                    `https://localhost:7260/api/Products/GetProductsByCategory/${categoryId}?pageNumber=${page}&pageSize=9`
                );
            } else {
                

                response = await fetch(
                    `https://localhost:7260/api/Products/GetAllProducts?pageNumber=${page}&pageSize=9`
                );
            }

            if (!response.ok) {
                throw new Error("Failed to fetch products");
            }

            auctionProducts = await response.json();
            renderProducts(auctionProducts, page);
        } catch (error) {
            console.error("Error fetching products:", error);
            document.getElementById(
                "auction-products-container"
            ).innerHTML = "<p>Error loading products</p>";
        }
    }

    function renderProducts(data, currentPage) {
        debugger
        const productsContainer = document.getElementById("auction-products-container");
        const paginationContainer = document.getElementById("pagination");

        if (!productsContainer || !paginationContainer) {
            console.error("Products or pagination container not found in the DOM.");
            return;
        }

        productsContainer.innerHTML = "";
        paginationContainer.innerHTML = "";

        if (!data.auctions || !Array.isArray(data.auctions) || data.auctions.length === 0) {
            productsContainer.innerHTML = "<p>No products found.</p>";
            return;
        }

        data.auctions.forEach(product => {
            productsContainer.innerHTML += `
                <div class="col-sm-6 col-lg-4">
                    <div class="auction-card">
                        <div class="card-image">
                            <img src="${(product.productDetails.imageUrl.startsWith("/images/")) 
                                ? `https://localhost:7260${product.productDetails.imageUrl}` 
                                : product.productDetails.imageUrl}" 
                                alt="auction-card-img">
                            <div class="timer-wrapper">
                                <div class="timer-inner" id="timer-${product.productDetails.productId}">
                                    <span class="days">0D</span>:<span class="hours">0H</span>:<span class="minutes">0M</span>:<span class="seconds">0S</span>
                                </div>
                            </div>
                        </div>
                        <div class="card-content">
                            <a href="bid-detail.html?auctionId=${product.auctionId}" class="card-title">
                                ${product.productDetails.productName}
                            </a>
                            <div class="d-flex justify-content-between align-items-center">
                                <p class="p-0">Current bid: <span>${product.currentHighestBid || product.productDetails.startingPrice}$</span></p>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        });

        startGlobalCountdown();
        generatePagination(data.totalPages, currentPage);
    }

    function generatePagination(totalPages, currentPage) {
        const paginationContainer = document.getElementById("pagination");
        if (!paginationContainer) return;

        paginationContainer.innerHTML = "";

        for (let i = 1; i <= totalPages; i++) {
            paginationContainer.innerHTML += `
                <li class="page-item ${i === currentPage ? "active" : ""}">
                    <a class="page-link" href="#">${i}</a>
                </li>
            `;
        }

        document.querySelectorAll(".page-link").forEach(link => {
            link.addEventListener("click", e => {
                e.preventDefault();
                const selectedPage = parseInt(e.target.textContent);
                document.getElementById("auction-products-container").innerHTML = "<p>Loading...</p>";
                fetchProducts(selectedPage);
            });
        });
    }

    function startGlobalCountdown() {
        auctionProducts.auctions.forEach(product => {
            startCountdown(`timer-${product.productDetails.productId}`, product.endTime);
        });
    }

    function startCountdown(timerId, endTime) {
        const timerElement = document.getElementById(timerId);

        if (!timerElement) return;

        const countdownInterval = setInterval(() => {
            const timeLeft = calculateTimeLeft(endTime);

            if (timeLeft.total <= 0) {
                clearInterval(countdownInterval);
                timerElement.innerHTML = "EXPIRED";
            } else {
                timerElement.innerHTML = `
                    <span class="days">${timeLeft.days}D</span>:<span class="hours">${timeLeft.hours}H</span>:<span class="minutes">${timeLeft.minutes}M</span>:<span class="seconds">${timeLeft.seconds}S</span>
                `;
            }
        }, 1000);
    }

    function calculateTimeLeft(endTime) {
        const end = new Date(endTime).getTime();
        const now = new Date().getTime();
        const difference = end - now;

        if (difference > 0) {
            return {
                total: difference,
                days: Math.floor(difference / (1000 * 60 * 60 * 24)),
                hours: Math.floor((difference / (1000 * 60 * 60)) % 24),
                minutes: Math.floor((difference / 1000 / 60) % 60),
                seconds: Math.floor((difference / 1000) % 60),
            };
        }

        return { total: 0, days: 0, hours: 0, minutes: 0, seconds: 0 };
    }

 
 

    getAllCategoryWithCount();
    fetchProducts();
});
