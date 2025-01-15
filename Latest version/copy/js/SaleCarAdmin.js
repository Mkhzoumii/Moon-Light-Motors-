// إضافة سيارة جديدة
async function addSaleCar() {
    const carName = document.getElementById('carName').value;
    const carModel = document.getElementById('carModel').value;
    const carPrice = document.getElementById('carPrice').value;
    const carImage = document.getElementById('carImage').files;
    const carDetails = document.getElementById('carDetails').value;
    const carFule = document.getElementById('carFule').value;
    const carDrivType = document.getElementById('carDrivType').value;
    const carTransnmission = document.getElementById('carTransnmission').value;
    const carPower = document.getElementById('carPower').value;
    const carConsumption = document.getElementById('carConsumption').value;

    // التحقق من إدخال جميع الحقول المطلوبة
    if (
        !carName ||
        !carModel ||
        !carPrice ||
        carImage.length === 0 ||
        !carDetails ||
        !carFule ||
        !carDrivType ||
        !carTransnmission ||
        !carPower ||
        !carConsumption
    ) {
        Swal.fire('Error', 'All fields are required', 'error');
        return;
    }

    // إنشاء FormData لإرسال البيانات مع الصور
    const formData = new FormData();
    formData.append('CarName', carName);
    formData.append('CarModel', carModel);
    formData.append('CarPrice', carPrice);
    formData.append('CarDetails', carDetails);
    formData.append('Fule', carFule);
    formData.append('DrivType', carDrivType);
    formData.append('Transnmission', carTransnmission);
    formData.append('Power', carPower);
    formData.append('Consumption', carConsumption);

    // إضافة الصور إلى الـ FormData
    for (const image of carImage) {
        formData.append('CarImage', image);
    }

    try {
        const response = await fetch('http://localhost:5002/Cars/addCar', {
            method: 'POST',
            body: formData,
        });

        if (response.ok) {
            Swal.fire('Success', 'Car added successfully!', 'success');
            loadCars(); // إعادة تحميل قائمة السيارات
        } else {
            const error = await response.text();
            Swal.fire('Error', `Failed to add car: ${error}`, 'error');
        }
    } catch (error) {
        console.error('Request Error:', error);
        Swal.fire('Error', 'Failed to add car. Please try again later.', 'error');
    }
}


// تحميل السيارات وعرضها في الجدول
async function loadCars() {
    try {
        const response = await fetch('http://localhost:5002/Cars/GetAvailableCars');
        const cars = await response.json();

        const tableBody = document.getElementById('carTableBody');
        tableBody.innerHTML = ''; // تنظيف الجدول

        cars.forEach(car => {
            const row = `
                <tr>
                    <td>${car.carID}</td>
                    <td>${car.carName}</td>
                    <td>${car.carModel}</td>
                    <td>${car.carPrice}</td>
                    <td>
                        <button class="btn btn-primary btn-sm" onclick='openEditModal(${JSON.stringify(car)})'>Edit</button>
                        <button class="btn btn-danger btn-sm" onclick='deleteCar(${car.carID})'>Delete</button>
                    </td>
                </tr>`;
            tableBody.insertAdjacentHTML('beforeend', row);
        });
    } catch (error) {
        Swal.fire('Error', 'Failed to load cars', 'error');
    }
}

// فتح المودال لتعديل السيارة
function openEditModal(car) {
    // تحديد الحقول
    document.getElementById('editCarId').value = car.carID || '';
    document.getElementById('editCarName').value = car.carName || '';
    document.getElementById('editCarModel').value = car.carModel || '';
    document.getElementById('editCarPrice').value = car.carPrice || '';
    document.getElementById('editCarDetails').value = car.carDetails || '';
    document.getElementById('editFuel').value = car.fule || '';
    document.getElementById('editDrivType').value = car.drivType || '';
    document.getElementById('editTransmission').value = car.transnmission || '';
    document.getElementById('editPower').value = car.power || '';
    document.getElementById('editConsumption').value = car.consumption || '';

    const editModal = new bootstrap.Modal(document.getElementById('editCarModal'));
    editModal.show();
}

// تعديل السيارة
async function updateCar(event) {
    event.preventDefault();

    const carId = parseInt(document.getElementById('editCarId').value, 10);
    const carName = document.getElementById('editCarName').value;
    const carModel = document.getElementById('editCarModel').value;
    const carPrice = parseFloat(document.getElementById('editCarPrice').value);
    const carDetails = document.getElementById('editCarDetails').value;
    const fuel = document.getElementById('editFuel').value;
    const driveType = document.getElementById('editDrivType').value;
    const transmission = document.getElementById('editTransmission').value;
    const power = document.getElementById('editPower').value;
    const consumption = document.getElementById('editConsumption').value;

    const carData = {
        carID: carId,
        carName: carName,
        carModel: carModel,
        carPrice: carPrice,
        carDetails: carDetails,
        userID: 0,
        drivType: driveType,
        power: power,
        fule: fuel,
        consumption: consumption,
        transnmission: transmission,
    };

    console.log('Data sent to API:', carData);

    try {
        const response = await fetch(`http://localhost:5002/Cars/edit-car/${carId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(carData),
        });

        if (response.ok) {
            Swal.fire('Success', 'Car updated successfully!', 'success');
            const editModal = bootstrap.Modal.getInstance(document.getElementById('editCarModal'));
            editModal.hide();
            loadCars();
        } else if (response.status === 405) {
            console.error('Error: Method Not Allowed');
            Swal.fire('Error', 'Method Not Allowed. Check API Documentation.', 'error');
        } else {
            const errorText = await response.text();
            console.error('API Error Response:', errorText);
            Swal.fire('Error', `Failed to update car: ${errorText}`, 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        Swal.fire('Error', 'Something went wrong. Please try again.', 'error');
    }
}


// حذف السيارة
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
            const response = await fetch(`http://localhost:5002/Cars/${carId}`, {
                method: 'DELETE',
            });

            if (response.ok) {
                Swal.fire('Deleted!', 'The car has been deleted.', 'success');
                loadCars();
            } else {
                Swal.fire('Error', 'Failed to delete car', 'error');
            }
        } catch (error) {
            Swal.fire('Error', 'Failed to delete car', 'error');
        }
    }
}

// تشغيل تحميل السيارات عند تحميل الصفحة
document.addEventListener('DOMContentLoaded', loadCars);

// ربط نموذج التعديل بدالة التحديث
document.getElementById('editCarForm').addEventListener('submit', updateCar);
