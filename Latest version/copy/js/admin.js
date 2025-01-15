// Function to add a car
async function addCar() {
    const carName = document.getElementById('carName').value;
    const carModel = document.getElementById('carModel').value;
    const carPrice = document.getElementById('carPrice').value;
    const carColor = document.getElementById('color').value;
    const carBodyType = document.getElementById('body').value;
    const carImages = document.getElementById('carImage').files;

    if (!carName || !carModel || !carPrice || !carColor || !carBodyType || carImages.length === 0) {
        Swal.fire('Error', 'All fields are required', 'error');
        return;
    }

    const formData = new FormData();
    formData.append('carname', carName);
    formData.append('model', carModel);
    formData.append('price', carPrice);
    formData.append('color', carColor);
    formData.append('body', carBodyType);

    // إضافة جميع الصور إلى FormData
    for (const image of carImages) {
        formData.append('images', image);
    }

    try {
        const response = await fetch('http://localhost:5002/CarsForRent/addCar', {
            method: 'POST',
            body: formData,
        });

        if (response.ok) {
            Swal.fire('Success', 'Car added successfully!', 'success');
            loadCars(); // Refresh table
        } else {
            const error = await response.text();
            Swal.fire('Error', `Failed to add car: ${error}`, 'error');
        }
    } catch (error) {
        Swal.fire('Error', 'Failed to add car. Please try again later.', 'error');
    }
}

// Function to fetch and display cars
async function loadCars() {
    try {
        const response = await fetch('http://localhost:5002/CarsForRent/GetCars');
        const cars = await response.json();

        const tableBody = document.getElementById('carTableBody');
        tableBody.innerHTML = ''; // Clear table before adding rows

        cars.forEach(car => {
            const row = `
                <tr>
                    <td>${car.carid}</td>
                    <td>${car.carname}</td>
                    <td>${car.model}</td>
                    <td>${car.price}</td>
                    <td>${car.color || 'N/A'}</td>
                    <td>${car.body || 'N/A'}</td>
                    <td>
                        <button class="btn btn-primary btn-sm" onclick='openEditModal(${JSON.stringify(car)})'>Edit</button>
                        <button class="btn btn-danger btn-sm" onclick="deleteCar(${car.carid})">Delete</button>
                    </td>
                </tr>`;
            tableBody.insertAdjacentHTML('beforeend', row);
        });
    } catch (error) {
        Swal.fire('Error', 'Failed to load cars', 'error');
    }
}

// Function to delete a car
async function deleteCar(carId) {
    const result = await Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to undo this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!',
    });

    if (result.isConfirmed) {
        try {
            const response = await fetch(`http://localhost:5002/CarsForRent/${carId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                },
            });

            if (response.ok) {
                Swal.fire('Deleted!', 'The car has been deleted.', 'success');
                loadCars(); // Refresh table
            } else {
                const error = await response.text();
                Swal.fire('Error', error, 'error');
            }
        } catch (error) {
            Swal.fire('Error', 'Failed to delete car', 'error');
        }
    }
}

// Function to open the Edit modal
function openEditModal(car) {
    console.log('Opening Edit Modal for car:', car);

    // Check if modal input fields exist before assigning values
    const carIdInput = document.getElementById('editCarId');
    const carNameInput = document.getElementById('editCarName');
    const carModelInput = document.getElementById('editCarModel');
    const carPriceInput = document.getElementById('editCarPrice');
    const carColorInput = document.getElementById('editCarColor');
    const carBodyInput = document.getElementById('editCarBodyType');

    if (carIdInput && carNameInput && carModelInput && carPriceInput && carColorInput && carBodyInput) {
        carIdInput.value = car.carid;
        carNameInput.value = car.carname;
        carModelInput.value = car.model;
        carPriceInput.value = car.price;
        carColorInput.value = car.color || '';
        carBodyInput.value = car.body || '';
    } else {
        console.error('Modal input fields not found');
    }

    const editModal = new bootstrap.Modal(document.getElementById('editCarModal'));
    editModal.show();
}

// Function to update car details
async function updateCar() {
    const carId = document.getElementById('editCarId').value;
    const carName = document.getElementById('editCarName').value;
    const carModel = document.getElementById('editCarModel').value;
    const carPrice = document.getElementById('editCarPrice').value;
    const carColor = document.getElementById('editCarColor').value;
    const carBodyType = document.getElementById('editCarBodyType').value;

    // Validate that all fields are filled
    if (!carName || !carModel || !carPrice || !carColor || !carBodyType) {
        Swal.fire('Error', 'All fields are required', 'error');
        return;
    }

    // Create the data object as per the API model
    const carData = {
        carid: parseInt(carId, 10), // Convert carId to integer
        carname: carName,           // Car name
        model: carModel,            // Car model
        price: parseFloat(carPrice), // Convert price to float
        color: carColor,            // Car color
        body: carBodyType           // Car body type
    };

    try {
        const response = await fetch(`http://localhost:5002/CarsForRent/edit-car/${carId}`, { // Append carId to URL
            method: 'PUT', // Use PUT method for updates
            headers: {
                'Content-Type': 'application/json', // Set content type as JSON
            },
            body: JSON.stringify(carData), // Send car data as a JSON string
        });

        if (response.ok) {
            // Show success message using SweetAlert
            Swal.fire({
                title: 'Success!',
                text: 'Car updated successfully.',
                icon: 'success',
                confirmButtonText: 'OK'
            }).then(() => {
                // Close the modal after SweetAlert confirmation
                const editModal = bootstrap.Modal.getInstance(document.getElementById('editCarModal'));
                editModal.hide();
            });
        } else {
            const error = await response.text();
            Swal.fire('Error', `Failed to update car: ${error}`, 'error');
        }
    } catch (error) {
        Swal.fire('Error', 'Something went wrong. Please try again.', 'error');
    }
}


// Load cars when the page is ready
document.addEventListener('DOMContentLoaded', loadCars);
