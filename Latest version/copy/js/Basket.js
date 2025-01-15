window.onload = () => {
    const urlParams = new URLSearchParams(window.location.search);
    const userId = localStorage.getItem("userId");
    const userName = localStorage.getItem("firstName");
    const userGreeting = document.querySelector(".nav-item .dropdown-toggle");

    if (userName && userGreeting) {
        userGreeting.textContent = `Welcome, ${userName}`;
    }

    if (userId && !isNaN(userId)) { // Validate that userId is not null or empty and is a number
        // Fetch the basket data using the userId from the API
        fetch(`http://localhost:5002/SpareParte/GetBasket?userId=${userId}`)
            .then(response => response.json())
            .then(data => {
                if (data && Array.isArray(data) && data.length > 0) {
                    renderBasket(data, userId); // Display the basket
                } else {
                    alert("No items found in the basket.");
                }
            })
            .catch(error => {
                console.error("Error fetching basket data:", error);
                alert("An error occurred while fetching the basket.");
            });
    } else {
        // If userId is not valid
        alert("Invalid user ID.");
    }
};

// Function to render basket data
function renderBasket(basketItems, userId) {
    const basketContainer = document.querySelector(".cart .title");
    const orderSummary = document.querySelector(".summary");
    basketContainer.nextElementSibling.innerHTML = ""; // Clear previous items

    let totalPrice = 0; // Total price for all items

    basketItems.forEach(item => {
        const itemTotalPrice = item.sparePrice * item.qty;
        totalPrice += itemTotalPrice;

        // HTML for each item
        const itemHTML = `
            <div class="row border-top border-bottom" id="basket-item-${item.spareId}">
                <div class="row main align-items-center">
                    <div class="col-2">
                        <img class="img-fluid" src="http://localhost:5002/SpareParte/GetSpareImage/${item.spareId}" alt="${item.sparePartName}">
                    </div>
                    <div class="col">
                        <div class="row text-muted">${item.sparePartName}</div>
                        <div class="row">Quantity: ${item.qty}</div>
                    </div>
                    <div class="col">
                        $${itemTotalPrice.toFixed(2)}
                        <span class="close text-danger" style="cursor: pointer;" onclick="confirmRemoveItem(${item.spareId}, ${userId})">
                            &#10005;
                        </span>
                    </div>
                </div>
            </div>
        `;
        basketContainer.nextElementSibling.insertAdjacentHTML("beforeend", itemHTML);
    });

    // Update order summary
    updateSummary(basketItems.length, totalPrice, userId);
}

// Function to update the order summary
function updateSummary(totalItems, totalPrice, userId) {
    const orderSummary = document.querySelector(".summary");

    const summaryHTML = `
        <div class="row">
            <div class="col" style="padding-left:0;">ITEMS ${totalItems}</div>
            <div class="col text-right">$${totalPrice.toFixed(2)}</div>
        </div>
        <div class="row">
            <hr>
            <div class="col">TOTAL PRICE</div>
            <div class="col text-right">$${totalPrice.toFixed(2)}</div>
        </div>
    `;
    orderSummary.innerHTML = summaryHTML;

    // Add purchase button
    const purchaseButton = `
        <div class="btn btn-secondary" onclick="completePurchase(${userId})">
            Purchase <span class="fas fa-arrow-right ps-2"></span>
        </div>
    `;
    orderSummary.insertAdjacentHTML("beforeend", purchaseButton);
}

// Function to update basket summary after removal
function updateBasketSummary(userId) {
    fetch(`http://localhost:5002/SpareParte/GetBasket?userId=${userId}`)
        .then(response => response.json())
        .then(data => {
            let totalPrice = 0;

            data.forEach(item => {
                totalPrice += item.sparePrice * item.qty;
            });

            updateSummary(data.length, totalPrice, userId);
        })
        .catch(error => {
            console.error("Error updating basket summary:", error);
        });
}

// Function to confirm item removal using SweetAlert
function confirmRemoveItem(spareId, userId) {
    Swal.fire({
        title: "Are you sure?",
        text: "Do you really want to remove this item from your basket?",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#d33",
        cancelButtonColor: "#3085d6",
        confirmButtonText: "Yes, remove it!",
        cancelButtonText: "Cancel",
    }).then((result) => {
        if (result.isConfirmed) {
            removeItemFromBasket(spareId, userId); // Call remove function
        }
    });
}

// Function to remove item from the basket
function removeItemFromBasket(spareId, userId) {
    fetch(`http://localhost:5002/SpareParte/RemoveFromBasket/${spareId}/${userId}`, {
        method: "DELETE"
    })
    .then(response => {
        if (response.ok) {
            Swal.fire("Deleted!", "The item has been removed from your basket.", "success");
            // Remove the item from DOM directly
            document.getElementById(`basket-item-${spareId}`).remove();
            updateBasketSummary(userId); // Update the basket summary after removal
        } else {
            Swal.fire("Error!", "Failed to remove the item. Please try again.", "error");
        }
    })
    .catch(error => {
        console.error("Error removing item:", error);
        Swal.fire("Error!", "An error occurred while removing the item.", "error");
    });
}

// Function to complete the purchase
function completePurchase(userId) {
    fetch(`http://localhost:5002/SpareParte/FinalizePurchase/${userId}`, {
        method: "POST"
    })
    .then(response => {
        if (response.ok) {
            Swal.fire("Success!", "Your purchase has been completed successfully.", "success").then(() => {
                // Clear basket UI after purchase
                clearBasketUI();
                updateBasketSummary(userId);

                // Redirect to order confirmation page
                window.location.href = "index.html"; 
            });
        } else {
            Swal.fire("Error!", "Failed to complete the purchase. Please try again.", "error");
        }
    })
    .catch(error => {
        console.error("Error completing purchase:", error);
        Swal.fire("Error!", "An error occurred while completing the purchase.", "error");
    });
}

// Function to clear basket UI after purchase
function clearBasketUI() {
    const basketContainer = document.querySelector(".cart .title");
    basketContainer.nextElementSibling.innerHTML = ""; // Clear items from UI
    updateSummary(0, 0, 0); // Update summary to show empty state
}
