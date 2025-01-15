document.addEventListener("DOMContentLoaded", () => {
    const cars = [
      { carID: 1, carName: "Toyota Corolla", carModel: "2023", carPrice: 20000 },
      { carID: 2, carName: "BMW X5", carModel: "2022", carPrice: 50000 },
      // Add more cars as needed
    ];
  
    const container = document.getElementById("cards-container");
  
    // Function to render car cards
    function renderCars() {
      cars.forEach((car) => {
        const cardHTML = `
          <div class="col-sm-6 col-md-4 col-lg-3">
            <div class="card">
              <div class="card-body">
                <h5 class="card-title">${car.carName}</h5>
                <p class="card-text">Model: ${car.carModel}</p>
                <p class="card-text">Price: $${car.carPrice}</p>
                <button class="btn btn-primary" onclick="openRentCarModal(${car.carID}, '${car.carName}')">Rent Now</button>
              </div>
            </div>
          </div>`;
        container.innerHTML += cardHTML;
      });
    }
  
    renderCars();
  
    // Function to open the rent car modal
    window.openRentCarModal = (carID, carName) => {
      document.getElementById("carId").value = carID;
      document.getElementById("carName").value = carName;
      const rentCarModal = new bootstrap.Modal(document.getElementById("rentCarModal"));
      rentCarModal.show();
    };
  
    // Handle form submission
    document.getElementById("rentCarForm").addEventListener("submit", (e) => {
      e.preventDefault();
      const carID = document.getElementById("carId").value;
      const startDate = document.getElementById("startDate").value;
      const endDate = document.getElementById("endDate").value;
  
      alert(`Car ID: ${carID}\nStart Date: ${startDate}\nEnd Date: ${endDate}`);
      const rentCarModal = bootstrap.Modal.getInstance(document.getElementById("rentCarModal"));
      rentCarModal.hide();
    });
  });
  