// Extract URL parameters
const urlParams = new URLSearchParams(window.location.search);
const carId = urlParams.get('data'); // Car ID
const carPrice = parseFloat(urlParams.get('price')); // Car Price (converted to number)
const sourcePage = urlParams.get('source'); // Source page (sale or rent)

// Debug: Log sourcePage and car price
console.log("Source Page:", sourcePage);
console.log("Car Price:", carPrice);
console.log("carId:", carId);

// DOM Elements
const img = document.getElementById("img");
const carName = document.getElementById("CarName");
const carDetails = document.getElementById("CarDetails");
const cardsContainer = document.querySelector('#cards-container');

// Determine API endpoint based on source page
const carDetailsUrl = sourcePage === 'sale'
    ? `http://localhost:5002/Cars/GetSinglecars/${carId}`
    : `http://localhost:5002/CarsForRent/GetSinglecars/${carId}`;

document.addEventListener('DOMContentLoaded', () => {
    const buildButton = document.getElementById('BUILD');
    buildButton.addEventListener('click', () => {
        if (sourcePage === 'rent') {
            const rentModal = new bootstrap.Modal(document.getElementById('rentModal'));
            rentModal.show();
        } else {
            // Pass car details to saleCar
            saleCar(carId, carName.textContent, carPrice);
        }
    });

    async function saleCar(carID, carName, carPrice) {
        const userId = localStorage.getItem("userId");

        if (!userId) {
            Swal.fire({
                icon: 'error',
                title: 'User Not Logged In',
                text: 'Please log in to proceed.',
                confirmButtonText: 'Login'
            });
            return;
        }

        // Show confirmation dialog with car details
        Swal.fire({
            title: "Confirm Purchase",
            html: ` 
                <p><strong>Car Name:</strong> ${carName}</p>
                <p><strong>Price:</strong> $${carPrice}</p>
            `,
            icon: "info",
            showCancelButton: true,
            confirmButtonText: "Proceed to Payment",
            cancelButtonText: "Cancel"
        }).then((result) => {
            if (result.isConfirmed) {
                // Redirect to payment page directly without calling the API
                window.location.href = `payment.html?carId=${carID}&userId=${userId}&price=${carPrice}&source=sale`;
            }
        });
    }

    const rentForm = document.getElementById('rentForm');
    rentForm.addEventListener('submit', async (event) => {
        event.preventDefault();

        // Retrieve input values
        const startDate = document.getElementById('startDate').value;
        const endDate = document.getElementById('endDate').value;

        // Validate dates
        if (new Date(startDate) > new Date(endDate)) {
            Swal.fire({
                icon: 'error',
                title: 'Invalid Date Range',
                text: 'End date must be after the start date.',
                confirmButtonText: 'Close'
            });
            return;
        }

        const userId = localStorage.getItem('userId');
        if (!userId) {
            Swal.fire({
                icon: 'error',
                title: 'User Not Logged In',
                text: 'Please log in to proceed.',
                confirmButtonText: 'Login'
            });
            return;
        }
        const requestData = {
            carid: parseInt(carId),
            start_rent: new Date(startDate).toISOString(),
            end_rent: new Date(endDate).toISOString(),
            price: carPrice,
        };
        
        try {
            const response = await fetch('http://localhost:5002/CarsForRent/CheckRentalConflict', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(requestData),
            });
        
            if (!response.ok) {
                throw new Error(`API Error: ${response.statusText} (status: ${response.status})`);
            }
        
            // قراءة الاستجابة
            const result = await response.json();
        
            console.log('Raw API Response:', result);  // طباعة الاستجابة في الكونسول
        
            // تحقق مما إذا كانت الاستجابة هي كائن
            if (result && typeof result === 'object') {
                console.log('API Response is an object:', result); // تسجيل الاستجابة ككائن
        
                // تحقق من وجود الحقول المطلوبة
                if (result.isAvailable !== undefined) {
                    if (result.isAvailable) {
                        const rentalDuration = result.totalDays; // عدد الأيام
                        const totalPrice = result.totalPrice; // السعر الإجمالي
        
                        // عرض SweetAlert عندما تكون السيارة متاحة
                        Swal.fire({
                            icon: 'success',
                            title: 'Car Available',
                            text: `The car is available for your selected dates.\nNumber of days: ${rentalDuration} days\nTotal Price: $${totalPrice}`,
                            confirmButtonText: 'Continue'
                        }).then(() => {
                            // إذا كان المستخدم اختار المتابعة
                            const startDate = document.getElementById('startDate').value;
                            const endDate = document.getElementById('endDate').value;
        
                            // إعادة توجيه المستخدم إلى صفحة الدفع
                            window.location.href = `payment.html?data=${carId}&price=${totalPrice}&source=rent&start=${startDate}&end=${endDate}`;
                        });
                    } else if (result.conflicts) {
                        const conflictMessages = result.conflicts.map(conflict =>
                            `From: ${new Date(conflict.start_rent).toLocaleDateString()} To: ${new Date(conflict.end_rent).toLocaleDateString()}`
                        ).join('\n');
        
                        // عرض رسالة إذا كان هناك تعارض
                        Swal.fire({
                            icon: 'error',
                            title: 'Car Not Available',
                            text: `The car is not available during the selected dates. Conflicts:\n${conflictMessages}`,
                            confirmButtonText: 'Close'
                        });
                    } else {
                        throw new Error('Unexpected response structure: Missing `isAvailable` or `conflicts` keys.');
                    }
                } else {
                    throw new Error('API response is not an object or missing necessary fields.');
                }
            } else {
                throw new Error('Unexpected response structure.');
            }
        
        } catch (error) {
            console.error('Error checking rental availability:', error);
            Swal.fire({
                icon: 'error',
                title: 'Unexpected Error',
                text: error.message || 'An unexpected error occurred. Please try again later.',
                confirmButtonText: 'Close'
            });
        }
        
        
    });
});

// Fetch and display car details
function getCarDetails() {
    fetch(carDetailsUrl)
        .then((res) => {
            if (!res.ok) {
                throw new Error('Network response was not ok');
            }
            return res.json();
        })
        .then((data) => {
            carName.textContent = sourcePage === 'sale' ? data.carName : data.carname;
            carDetails.textContent = sourcePage === 'sale' ? data.carDetails : data.model;
            
            // For Rent: Fetch the image IDs and load the images one by one
            if (sourcePage === 'rent') {
                fetchImagesByCarId(carId);
            } else {
                // For Sale: Fetch the images for sale cars
                fetchImagesForSale(carId); // Fetch images for sale cars using the new function
            }
        })
        .catch((error) => {
            console.error('Error fetching car details:', error);
            carDetails.textContent = "Error loading car details.";
        });
}

getCarDetails();

// // Fetch and display related products
// function generateCardDetails(categoryId = null) {
//     const url = "http://localhost:5002/Cars/GetCars";

//     fetch(url)
//         .then((res) => res.json())
//         .then((data) => {
//             // Clear existing cards
//             cardsContainer.innerHTML = '';

//             // Filter cars by category if specified
//             let filteredCars = categoryId
//                 ? data.filter(car => car.categoryId === categoryId)
//                 : data;

//             // Limit to first 5 cars
//             filteredCars = filteredCars.slice(0, 5);

//             // Handle empty data
//             if (filteredCars.length === 0) {
//                 cardsContainer.innerHTML = '<p class="text-center">No related products found.</p>';
//                 return;
//             }

//             // Render cards dynamically
//             filteredCars.forEach(car => {
//                 const cardHTML = `
//                     <div class="col-sm-6 col-md-4 col-lg-3 box">
//                         <div class="card">
//                             <img src="http://localhost:5002/Cars/GetImageCar/${car.carID}" class="card-img-top" alt="${car.carName}">
//                             <div class="card-body pt-3 px-0">
//                                 <div class="car-name mb-0 mt-0 px-3"><h4>${car.carName}</h4></div>
//                                 <div class="d-flex justify-content-between mb-0 px-3">
//                                     <small class="text-muted mt-1">STARTING AT</small>
//                                     <h6>${car.carPrice}</h6>
//                                 </div>
//                                 <a href="/carDetails.html?data=${car.carID}&price=${car.carPrice}&source=sale">
//                                     <div class="text-center mt-3 px-2"><button class="btn btn-outline-secondary btn-block">View</button></div>
//                                 </a>
//                             </div>
//                         </div>
//                     </div>`;
//                 cardsContainer.innerHTML += cardHTML;
//             });
//         })
//         .catch((err) => console.log(err));
// }

// generateCardDetails();

// Fetch the image IDs for car rental
function fetchImagesByCarId(carId) {
    fetch(`http://localhost:5002/CarsForRent/get-images-by-carid/${carId}`)
        .then((res) => res.json())
        .then((data) => {
            const imageIds = data.imageIds;
            if (imageIds && imageIds.length > 0) {
                let currentImageIndex = 0;
                loadImage(imageIds[currentImageIndex]);

                // Add event listener to the previous button
                const prevButton = document.getElementById('prev-button');
                prevButton.addEventListener('click', () => {
                    currentImageIndex = (currentImageIndex - 1 + imageIds.length) % imageIds.length;
                    loadImage(imageIds[currentImageIndex]);
                });

                // Add event listener to the next button
                const nextButton = document.getElementById('next-button');
                nextButton.addEventListener('click', () => {
                    currentImageIndex = (currentImageIndex + 1) % imageIds.length;
                    loadImage(imageIds[currentImageIndex]);
                });
            } else {
                img.src = 'path/to/default/image'; // Default image if no images are available
            }
        })
        .catch((error) => {
            console.error('Error fetching image IDs for rent:', error);
            img.src = 'path/to/default/image'; // Default image if error occurs
        });
}

// Function to load car images for rental
function loadImage(imageId) {
    fetch(`http://localhost:5002/CarsForRent/get-image-by-id/${imageId}`)
        .then((res) => {
            if (!res.ok) {
                throw new Error('Failed to load image');
            }
            return res.blob(); // Convert the response to a blob (image data)
        })
        .then((blob) => {
            // Create an object URL for the image blob
            const imageUrl = URL.createObjectURL(blob);
            img.src = imageUrl; // Set the image source to the object URL
        })
        .catch((error) => {
            console.error('Error fetching image by id for rent:', error);
        });
}

// Function to load sale car images
function loadSaleImage(imageId) {
    fetch(`http://localhost:5002/Cars/get-image-by-id/${imageId}`)
        .then((res) => {
            if (!res.ok) {
                throw new Error('Failed to load image');
            }
            return res.blob(); // Convert the response to a blob (image data)
        })
        .then((blob) => {
            // Create an object URL for the image blob
            const imageUrl = URL.createObjectURL(blob);
            img.src = imageUrl; // Set the image source to the object URL
        })
        .catch((error) => {
            console.error('Error fetching image by id for sale:', error);
        });
}

// Fetch and display images for sale cars
// Fetch and display images for sale cars
function fetchImagesForSale(carId) {
    fetch(`http://localhost:5002/Cars/get-images-by-carid/${carId}`)
        .then((res) => res.json())
        .then((data) => {
            const imageIds = data.imageIds;
            console.log('Image IDs:', imageIds); // Debug
            if (imageIds && imageIds.length > 0) {
                let currentImageIndex = 0;

                // Load the first image
                loadSaleImage(imageIds[currentImageIndex]);

                // Add event listeners
                const prevButton = document.getElementById('prev-button');
                const nextButton = document.getElementById('next-button');

                if (prevButton && nextButton) {
                    prevButton.addEventListener('click', () => {
                        currentImageIndex =
                            (currentImageIndex - 1 + imageIds.length) % imageIds.length;
                        console.log('Previous Button Clicked, Current Index:', currentImageIndex); // Debug
                        loadSaleImage(imageIds[currentImageIndex]);
                    });

                    nextButton.addEventListener('click', () => {
                        currentImageIndex =
                            (currentImageIndex + 1) % imageIds.length;
                        console.log('Next Button Clicked, Current Index:', currentImageIndex); // Debug
                        loadSaleImage(imageIds[currentImageIndex]);
                    });
                } else {
                    console.error('Navigation buttons not found in DOM.');
                }
            } else {
                console.warn('No images found for this car.');
                img.src = 'path/to/default/image'; // Default image
            }
        })
        .catch((error) => {
            console.error('Error fetching image IDs for sale:', error);
            img.src = 'path/to/default/image'; // Default image
        });
}

// Function to load sale car images
function loadSaleImage(imageId) {
    console.log('Loading Image ID:', imageId); // Debug
    fetch(`http://localhost:5002/Cars/get-image-by-id/${imageId}`)
        .then((res) => {
            if (!res.ok) {
                throw new Error('Failed to load image');
            }
            return res.blob(); // Convert the response to a blob (image data)
        })
        .then((blob) => {
            const imageUrl = URL.createObjectURL(blob);
            console.log('Image URL:', imageUrl); // Debug
            img.src = imageUrl; // Set the image source to the object URL
        })
        .catch((error) => {
            console.error('Error fetching image by ID for sale:', error);
        });
}




































































































































