document.addEventListener("DOMContentLoaded", () => {
  // Extract URL parameters
  const urlParams = new URLSearchParams(window.location.search);
  const carIdForRent = parseInt(urlParams.get("data")); // Car ID for rent
  const carIdForSale = parseInt(urlParams.get("carId")); // Car ID for sale
  const carPrice = parseFloat(urlParams.get("price")); // Car Price
  const sourcePage = urlParams.get("source"); // Source page (sale or rent)
  const startDate = urlParams.get("start"); // Start date (for rent)
  const endDate = urlParams.get("end"); // End date (for rent)
  const paymentDate = new Date().toISOString();

  // DOM Elements for displaying car details
  const carImage = document.getElementById("carImage");
  const carName = document.getElementById("carName");
  const carModel = document.getElementById("carModel");
  const carColor = document.getElementById("carColor"); // Will be hidden for sale cars
  const carBody = document.getElementById("carBody"); // Will be removed
  const Subtotal = document.getElementById("Subtotal"); // Subtotal element

  // Set Subtotal = Price
  if (Subtotal) {
    Subtotal.textContent = `$${carPrice.toFixed(2)}`;
  }

  // Fetch and display the first image for cars (sale or rent)
  function fetchFirstImageId(carId, imgElementId, endpointBase) {
    fetch(`${endpointBase}/get-images-by-carid/${carId}`)
      .then((res) => res.json())
      .then((data) => {
        const imageIds = data.imageIds;
        if (imageIds && imageIds.length > 0) {
          const firstImageId = imageIds[0];
          loadImage(firstImageId, imgElementId, endpointBase);
        } else {
          document.getElementById(imgElementId).src = "path/to/default/image.jpg"; // Default image path
        }
      })
      .catch((error) => {
        console.error("Error fetching image IDs:", error);
        document.getElementById(imgElementId).src = "path/to/default/image.jpg"; // Default image path
      });
  }

  // Function to load and display an image by imageId using get-image-by-id
  function loadImage(imageId, imgElementId, endpointBase) {
    fetch(`${endpointBase}/get-image-by-id/${imageId}`)
      .then((res) => {
        if (!res.ok) {
          throw new Error("Failed to load image");
        }
        return res.blob();
      })
      .then((blob) => {
        const imageUrl = URL.createObjectURL(blob);
        document.getElementById(imgElementId).src = imageUrl;
      })
      .catch((error) => {
        console.error("Error loading image:", error);
        document.getElementById(imgElementId).src = "path/to/default/image.jpg"; // Default image path
      });
  }

  // Determine the API endpoint based on the source page (sale or rent)
  const carDetailsUrl =
    sourcePage === "sale"
      ? `http://localhost:5002/Cars/GetSinglecars/${carIdForSale}`
      : `http://localhost:5002/CarsForRent/GetSinglecars/${carIdForRent}`;

  // Fetch car details
  async function getCarDetails() {
    try {
      const res = await fetch(carDetailsUrl);

      if (!res.ok) {
        throw new Error("Failed to fetch car details.");
      }

      const data = await res.json();

      if (sourcePage === "sale") {
        // For sale, fetch and set the first image
        fetchFirstImageId(carIdForSale, "carImage", "http://localhost:5002/Cars");

        // Hide color and remove body
        if (carColor) {
          carColor.style.display = "none"; // Hide color element
        }
        if (carBody) {
          carBody.style.display = "none"; // Completely remove body element
        }

        // Display price directly
        carModel.textContent += ` | Price: $${carPrice.toFixed(2)}`;
      } else {
        // For rent, fetch and set the first image
        fetchFirstImageId(carIdForRent, "carImage", "http://localhost:5002/CarsForRent");

        // Show details for rent
        carColor.textContent = data.color || "red"; // Default to 'red'
        carBody.textContent = data.body || "coupe"; // Default to 'coupe'
        carColor.style.display = ""; // Ensure the element is visible for rent cars
        carBody.style.display = ""; // Ensure the element is visible for rent cars
      }

      carName.textContent =
        sourcePage === "rent" ? data.carname || "Unknown Car" : data.carName || "Unknown Car";
      carModel.textContent =
        sourcePage === "rent" ? data.model || "Unknown Model" : data.carModel || "Unknown Model";
    } catch (error) {
      console.error("Error fetching car details:", error);
      carName.textContent = "Error loading car details.";
      carImage.src = "path/to/default/image.jpg"; // Default image path
    }
  }

  // Call the function to fetch and display details
  getCarDetails();

  // Payment processing
  const submitPaymentButton = document.getElementById("submitPaymentButton");
  if (submitPaymentButton) {
    submitPaymentButton.addEventListener("click", async (event) => {
      event.preventDefault(); // Prevent form submission

      const userId = parseInt(localStorage.getItem("userId"));
      if (!userId) {
        Swal.fire({
          icon: "error",
          title: "User Not Logged In",
          text: "Please log in to complete the payment.",
          confirmButtonText: "Login",
        });
        return;
      }

      // Validate dates for rentals
      if (sourcePage === "rent" && (!startDate || !endDate)) {
        Swal.fire({
          icon: "error",
          title: "Invalid Dates",
          text: "Please provide both start and end dates for the rental.",
          confirmButtonText: "Close",
        });
        return;
      }

      // Create payment request payload
      const paymentData = {
        carid: sourcePage === "rent" ? carIdForRent : carIdForSale,
        UserID: userId,
        payment_date: paymentDate,
        price: carPrice,
        payment_type: sourcePage, // "sale" or "rent"
      };

      if (sourcePage === "rent") {
        paymentData.start_rent = startDate;
        paymentData.end_rent = endDate;
      }

      try {
        const response = await fetch(
          sourcePage === "sale"
            ? "http://localhost:5002/Cars/AddSale"
            : "http://localhost:5002/CarsForRent/AddRental",
          {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify(paymentData),
          }
        );

        if (!response.ok) {
          const errorResponse = await response.text();
          Swal.fire({
            icon: "error",
            title: "Payment Failed",
            text: errorResponse || "Something went wrong while processing your payment.",
            confirmButtonText: "Close",
          });
          return;
        }

        const result = await response.json();
        Swal.fire({
          icon: "success",
          title: "Payment Success",
          text: result.Message || "Payment has been successfully processed.",
          confirmButtonText: "Close",
        }).then(() => {
          window.location.href = "index.html";
        });
      } catch (error) {
        console.error("Error processing payment:", error);
        Swal.fire({
          icon: "error",
          title: "Unexpected Error",
          text: "An unexpected error occurred while processing your payment. Please try again.",
          confirmButtonText: "Close",
        });
      }
    });
  } else {
    console.error("Payment button not found");
  }
});
