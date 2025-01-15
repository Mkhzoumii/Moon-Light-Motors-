// Assuming you have included SweetAlert2 in your project
// For example, include this in your HTML <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

// Get userId from localStorage
const userId = localStorage.getItem('userId');
console.log("userId:", userId);

// Function to check availability when clicking Book Now
function checkAvailability() {
    // Get form values
    const start_date = document.getElementById('startDate').value; // Start date input
    const maintenance_type = document.querySelector('.dropdown-menu .dropdown-item.active')?.textContent; // Get the active dropdown item

    // Check if necessary data is provided
    if (!start_date || !maintenance_type) {
        // SweetAlert for missing fields
        Swal.fire({
            icon: 'error',
            title: 'Oops!',
            text: 'Please fill in all the fields.',
        });
        return;
    }

    // Ensure start date is not in the past
    if (new Date(start_date) < new Date()) {
        // SweetAlert for start date in the past
        Swal.fire({
            icon: 'error',
            title: 'Invalid Start Date!',
            text: 'Start date cannot be in the past.',
        });
        return;
    }

    // Check if userId is available in localStorage
    if (!userId || userId.trim() === '') {
        // SweetAlert for missing or invalid userId (not logged in)
        Swal.fire({
            icon: 'error',
            title: 'Not Logged In!',
            text: 'You must be logged in to make a booking.',
        });
        return;
    }

    // Create the data object to send, including userId
    const formData = {
        userId: userId, // Add the userId here
        start_date: start_date,  // Start date
        maintenance_type: maintenance_type, // Selected service
    };

    // API URL (change this to your actual API endpoint)
    const apiUrl = 'http://localhost:5002/Maintenance/CheckAvailability'; // Assuming this endpoint checks availability

    // Send the data to the API using Fetch API (POST method)
    fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(formData),
    })
        .then(response => {
            // Check if the response status is OK (2xx)
            if (!response.ok) {
                throw new Error('Network response was not ok ' + response.statusText);
            }
            // Check if the response has content
            return response.text().then(text => text ? JSON.parse(text) : {});
        })
        .then(data => {
            // Check if the time is available or not
            if (data.message === "The requested time is available.") {
                Swal.fire({
                    icon: 'success',
                    title: 'Time Available!',
                    text: `The requested time is available. Maintenance Time: ${data.maintenance_time}, Price: ${data.maintenance_price}`,
                }).then(() => {
                    // Show confirmation for booking
                    confirmBooking(formData, data.maintenance_time, data.maintenance_price);
                });
            } else {
                // If time is not available, show error with conflicting end time
                Swal.fire({
                    icon: 'error',
                    title: 'Time Not Available!',
                    text: `The requested time is not available. Please try another time. The last conflicting appointment ends at: ${data.last_end_time}`,
                });
            }
        })
        .catch((error) => {
            // Handle other errors (failure)
            console.error('Error:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error!',
                text: 'An error occurred. Please try again later.',
            });
        });
}

// Function to book the maintenance
function confirmBooking(formData, maintenanceTime, maintenancePrice) {
    // Now, confirm booking and send data to the backend to store the booking
    const bookingData = {
        userId: formData.userId,
        start_date: formData.start_date,
        maintenance_type: formData.maintenance_type,
        maintenance_time: maintenanceTime, // Maintenance time duration
        maintenance_price: maintenancePrice, // Maintenance price
    };

    const apiUrl = 'http://localhost:5002/Maintenance/add-maintenance'; // API endpoint to add the maintenance

    fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(bookingData),
    })
    .then(response => {
        // Check if the response status is OK (2xx)
        if (!response.ok) {
            throw new Error('Network response was not ok ' + response.statusText);
        }
        // Check if the response has content
        return response.text().then(text => text ? JSON.parse(text) : {});
    })
    .then(data => {
        // If booking is successful, show confirmation
        if (data.success) {
            Swal.fire({
                icon: 'success',
                title: 'Booking Confirmed!',
                text: `Your maintenance appointment has been successfully booked. Start time: ${data.maintenance.start_date}, End time: ${data.maintenance.end_date}, Price: ${data.maintenance.price}`,
            });
        } else {
            // Handle the case where success is false
            Swal.fire({
                icon: 'error',
                title: 'Error!',
                text: 'An error occurred while booking. Please try again later.',
            });
        }
    })
    .catch((error) => {
        // Handle any errors in booking
        console.error('Error:', error);
        Swal.fire({
            icon: 'error',
            title: 'Error!',
            text: 'An error occurred while booking. Please try again later.',
        });
    });
}

// Attach the checkAvailability function to the Book Now button click event
document.addEventListener('DOMContentLoaded', function() {
    const bookNowButton = document.getElementById('bookNowButton');  // Ensure this button has the correct ID
    if (bookNowButton) {  // Check if the button exists
        bookNowButton.addEventListener('click', function(event) {
            event.preventDefault(); // Prevent any default behavior for the button
            checkAvailability(); // Call the checkAvailability function when the button is clicked
        });
    }

    // Event listener to update active class for dropdown items
    const dropdownItems = document.querySelectorAll('.dropdown-item');
    dropdownItems.forEach(item => {
        item.addEventListener('click', function() {
            // Remove active class from all items
            dropdownItems.forEach(i => i.classList.remove('active'));
            // Add active class to clicked item
            item.classList.add('active');
        });
    });
});
