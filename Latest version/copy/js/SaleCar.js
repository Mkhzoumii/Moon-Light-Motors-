$(document).ready(function () {
    let allCars = []; // Store all cars fetched from the server
    let filteredCars = []; // Store filtered cars based on search/sort

    // Function to fetch all cars
    function fetchAllCars() {
        fetch("http://localhost:5002/Cars/GetAvailableCars")
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

    // Function to fetch and display the first image for a car using get-images-by-carid
    function fetchFirstImageId(carId, imgElementId) {
        fetch(`http://localhost:5002/Cars/get-images-by-carid/${carId}`)
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

    // Function to load and display an image by imageId using get-image-by-id
    function loadImage(imageId, imgElementId) {
        fetch(`http://localhost:5002/Cars/get-image-by-id/${imageId}`)
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
                document.getElementById(imgElementId).src = 'path/to/default/image.jpg'; // Default image path
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
                        <img id="img-${car.carID}" class="card-img-top" alt="${car.carName}" width="100%">
                        <div class="card-body pt-3 px-0">
                            <div class="car-name mb-0 mt-0 px-3">
                                <h4>${car.carName}</h4>
                            </div>
                            <div class="d-flex justify-content-between mb-0 px-3">
                                <small class="text-muted mt-1">MODEL</small>
                                <h6>${car.carModel}</h6>
                            </div>
                            <div class="d-flex justify-content-between mb-0 px-3">
                                <small class="text-muted mt-1">STARTING AT</small>
                                <h6>${car.carPrice}</h6>
                            </div>
                            <hr class="mt-2 mx-3">
                            
                           
                            <div class="mx-3 mt-3 mb-2">
                                <button type="button" class="btn btn-danger btn-block" onclick="storeCarDetails(${car.carID})">
                                    <small>Details</small>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>`;
            container.append(cardHTML);

            // Fetch and display the first image for each car using get-images-by-carid
            fetchFirstImageId(car.carID, `img-${car.carID}`);
        });
    }

    // Store car details in localStorage
    window.storeCarDetails = function (carID) {
        const selectedCar = allCars.find((car) => car.carID === carID);
        if (selectedCar) {
            const carPrice = selectedCar.carPrice;
            window.location.href = `details.html?data=${carID}&price=${carPrice}&source=sale`;
        } else {
            console.error("Car not found");
        }
    };

    // Filter cars by search query
    function filterCarsByName(query) {
        filteredCars = allCars.filter((car) =>
            car.carName.toLowerCase().includes(query.toLowerCase())
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
        filteredCars.sort((a, b) => (order === "lowToHigh" ? a.carPrice - b.carPrice : b.carPrice - a.carPrice));
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
