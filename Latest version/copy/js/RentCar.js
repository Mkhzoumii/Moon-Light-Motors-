// // Function to check if the user is authenticated
// function isAuthenticated() {
//   // Check if a token exists in localStorage
//   const token = localStorage.getItem("authToken");
//   return token !== null; // Returns true if token exists, false otherwise
// }

// // Redirect unauthenticated users to login page
// function protectRoute() {
//   if (!isAuthenticated()) {
//     // If not authenticated, redirect to login page
//     window.location.href = "Login.html";
//   }
// }

// // Run protectRoute on page load
// window.onload = function () {
//   protectRoute(); // Check authentication when the page loads
// };
// // Logout function to clear token and redirect to login page
// function logout() {
//   // Remove the auth token from localStorage
//   localStorage.removeItem("authToken");
//   // Redirect to login page
//   window.location.href = "Login.html";
// }
// function EditCar(carID) {
//   var UserID = localStorage.getItem("userId");

//   const EditData = {
//     carID: carID,
//     UserID: UserID,
//   };

//   const url = `http://localhost:5002/Cars/EditCar`;
//   fetch(url, {
//     method: "PUT",
//     headers: {
//       "Content-Type": "application/json",
//     },
//     body: JSON.stringify(EditData),
//   })
//     .then((response) => {
//       if (response.ok) {
//         return response.text(); // Use response.text() for empty responses
//       } else {
//         throw new Error("Error editing car");
//       }
//     })
//     .then((data) => {
//       //console.log('Car edited successfully:', data);
//       alert("Car edited successfully");
//     })
//     .catch((error) => {
//       console.error("Error:", error);
//     });
// }

// const url = "http://localhost:5002/CarsForRent/GetCars";

// fetch(url)
//   .then((res) => res.json())
//   .then((data) => {
//     const cardsContainer = document.getElementById("cards-container");
//     cardsContainer.innerHTML = ""; // Clear previous content

//     data.forEach((car) => {
//       const cardHTML = `
//         <div class="col-sm-6 col-md-4 col-lg-3 box">
//           <div class="card">
//             <img src="http://localhost:5002/CarsForRent/GetImageCar/${car.carid}" class="card-img-top" width="100%">
//             <div class="card-body pt-3 px-0">
//               <div class="car-name mb-0 mt-0 px-3">
//                 <h4>${car.carname}</h4>
//               </div>
//               <div class="d-flex justify-content-between mb-0 px-3">
//                 <small class="text-muted mt-1">MODEL</small>
//                 <h6>${car.model}</h6>
//               </div>
//               <div class="d-flex justify-content-between mb-0 px-3">
//                 <small class="text-muted mt-1">STARTING AT</small>
//                 <h6>${car.price}</h6>
//               </div>
//               <hr class="mt-2 mx-3">
//               <div class="d-flex justify-content-between px-3 pb-4">
//                 <div class="d-flex flex-column">
//                   <h5 class="mb-0">Car Number: ${car.carid}</h5>
//                 </div>
//               </div>
//               <div class="mx-3 mt-3 mb-2">
//                 <button type="button" class="btn btn-danger btn-block" onclick="RentCar(${car.carid})">
//                   <small>RENT NOW</small>
//                 </button>
//               </div>
//               <div class="mx-3 mt-3 mb-2">
//                 <button type="button" class="btn btn-danger btn-block" onclick="openDetails(${car.carid}, 'rent')">
//                   <small>DETAILS</small>
//                 </button>
//               </div>
//             </div>
//           </div>
//         </div>`;

//       cardsContainer.insertAdjacentHTML("beforeend", cardHTML);
//     });
//   })
//   .catch((error) => {
//     console.error("Error fetching data:", error);
//     const cardsContainer = document.getElementById("cards-container");
//     cardsContainer.innerHTML = '<p class="text-center text-danger">Error loading products.</p>';
//   });


// function RentCar() 
// {
//   const url = "http://localhost:5002/CarsForRent/GetCars";
// fetch(url)
//   .then((res) => res.json())
//   .then((data) => {
//     cardsContainer.innerHTML = ''; 
//     data
//     // .filter(category => category.id === categoryId)
//     .forEach(car => {
//       // for (let i = 0; i < 6; i++) {
//         const cardHTML = `
//           <div class="col-sm-6 col-md-4 col-lg-3 box">
//             <div class="card">
//               <img src="http://localhost:5002/CarsForRent/GetImageCar/${car.carid}" class="card-img-top" width="100%">
//               <div class="card-body pt-3 px-0">
//                 <div class="car-name mb-0 mt-0 px-3"><h4>${car.carname}</h4></div>
//                 <div class="d-flex justify-content-between mb-0 px-3">
//                   <small class="text-muted mt-1">STARTING AT</small>
//                   <h6>${car.price}</h6>
//                 </div>
//                 <hr class="mt-2 mx-3">
//                 <div class="d-flex justify-content-between px-3 pb-4">
                  
//                   <div class="d-flex flex-column">
//                     <h5 class="mb-0">${car.model}</h5>
//                   </div>
//                 </div>
//                 <div class="mx-3 mt-3 mb-2">
//                   <button type="button" class="btn btn-danger btn-block">
//                     <small>BUILD & PRICE</small>
//                   </button>
//                 </div>
//                  <div class="mx-3 mt-3 mb-2">
//                   <button type="button"  class="btn btn-danger btn-block" onclick="openDetails(${car.carid}, 'rent')">
//                     <small>Details</small>
//                   </button>
//               </div>
//             </div>
//           </div>`;
//         cardsContainer.insertAdjacentHTML('beforeend', cardHTML);
//       // }
//     });
//   })
// }

$(document).ready(function () {
    let allCars = []; // Store all cars fetched from the server
    let filteredCars = []; // Store filtered cars based on search/sort

    // Function to fetch all cars
    function fetchAllCars() {
        fetch("http://localhost:5002/CarsForRent/GetCars")
            .then((res) => res.json())
            .then((data) => {
                allCars = data; // Store all cars
                filteredCars = [...data]; // Initially, filteredCars is the same as allCars
                renderCars(data);
            })
            .catch((error) => {
                console.error("Error fetching all cars:", error);
                $("#cards-container").html('<p class="text-center text-danger">Error loading cars.</p>');
            });
    }

    // Function to fetch and display the first image for a car
    function fetchFirstImageId(carId, imgElementId) {
        fetch(`http://localhost:5002/CarsForRent/get-images-by-carid/${carId}`)
            .then((res) => res.json())
            .then((data) => {
                const imageIds = data.imageIds;
                if (imageIds && imageIds.length > 0) {
                    const firstImageId = imageIds[0];
                    loadImage(firstImageId, imgElementId);
                } else {
                    document.getElementById(imgElementId).src = 'path/to/default/image.jpg'; // Default image path
                }
            })
            .catch((error) => {
                console.error("Error fetching image IDs:", error);
                document.getElementById(imgElementId).src = 'path/to/default/image.jpg'; // Default image path
            });
    }

    // Function to load and display an image by ID
    function loadImage(imageId, imgElementId) {
        fetch(`http://localhost:5002/CarsForRent/get-image-by-id/${imageId}`)
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
            });
    }

    // Function to render cars
    function renderCars(cars) {
        const container = $("#cards-container");
        container.empty();

        if (cars.length === 0) {
            container.html('<p class="text-center text-danger">No cars available.</p>');
            return;
        }

        cars.forEach((car) => {
            const cardHTML = `
                <div class="col-sm-6 col-md-4 col-lg-3 box">
                    <div class="card">
                        <img id="img-${car.carid}" class="card-img-top" alt="${car.carname}" width="100%">
                        <div class="card-body pt-3 px-0">
                            <div class="car-name mb-0 mt-0 px-3">
                                <h4>${car.carname}</h4>
                            </div>
                            <div class="d-flex justify-content-between mb-0 px-3">
                                <small class="text-muted mt-1">MODEL</small>
                                <h6>${car.model}</h6>
                            </div>
                            <div class="d-flex justify-content-between mb-0 px-3">
                                <small class="text-muted mt-1">STARTING AT</small>
                                <h6>${car.price}</h6>
                            </div>
                            <hr class="mt-2 mx-3">
                            <div class="d-flex justify-content-between px-3 pb-4">
                                <div class="d-flex flex-column">
                                    <h5 class="mb-0">Car Number: ${car.carid}</h5>
                                </div>
                            </div>
                            <div class="mx-3 mt-3 mb-2">
                                <button type="button" class="btn btn-danger btn-block" onclick="storeCarDetails(${car.carid})">
                                    <small>Details</small>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>`;
            container.append(cardHTML);

            // Fetch and display the first image for each car
            fetchFirstImageId(car.carid, `img-${car.carid}`);
        });
    }

    // Store car details in localStorage
    window.storeCarDetails = function (carId) {
        const selectedCar = allCars.find((car) => car.carid === carId);
        if (selectedCar) {
            const carPrice = selectedCar.price;
            window.location.href = `details.html?data=${carId}&price=${carPrice}&source=rent`;
        } else {
            console.error("Car not found");
        }
    };

    // Filter cars by search query
    function filterCarsByName(query) {
        filteredCars = allCars.filter((car) =>
            car.carname.toLowerCase().includes(query.toLowerCase())
        );
        renderCars(filteredCars);
    }
    $("#carSearch").on("input", function () {
        const query = $(this).val().trim();
        if (query.length >= 2) {
            filterCarsByName(query);
        } else {
            renderCars(allCars); // Show all cars when query is cleared
        }
    });

    // Sort cars by price
    function sortCars(order) {
        filteredCars.sort((a, b) => (order === "lowToHigh" ? a.price - b.price : b.price - a.price));
        renderCars(filteredCars);
    }

    // Event listeners for sorting
    $(".dropdown-item").click(function () {
        const sortType = $(this).text().trim();
        if (sortType === "Low to high") {
            sortCars("lowToHigh");
        } else if (sortType === "High to low") {
            sortCars("highToLow");
        }
    });

    // Fetch all cars on page load
    fetchAllCars();
});
