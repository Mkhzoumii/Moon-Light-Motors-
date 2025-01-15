// استدعاء البيانات لعرض القطع في الجدول
function fetchSpareParts() {
    fetch('http://localhost:5002/SpareParte/GetSpareParte')
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            const tableBody = document.getElementById('carTableBody');
            console.log('Fetched data:', data);
            tableBody.innerHTML = ''; // تنظيف الجدول قبل إعادة تعبئته
            if (data.length === 0) {
                const emptyRow = document.createElement('tr');
                emptyRow.innerHTML = `<td colspan="6" class="text-center">No spare parts available.</td>`;
                tableBody.appendChild(emptyRow);
                return;
            }
            data.forEach(sparePart => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${sparePart.spareId}</td>
                    <td>${sparePart.spareName}</td>
                    <td>${sparePart.spareModel}</td>
                    <td>${sparePart.sparePrice}</td>
                    <td>${sparePart.qty}</td>
                    <td>
                        <button class="btn btn-primary btn-sm" onclick='openEditModal(${JSON.stringify(sparePart)})'>Edit</button>
                        <button class="btn btn-danger btn-sm" onclick="deleteSparePart(${sparePart.spareId})">Delete</button>
                    </td>
                `;
                tableBody.appendChild(row);
            });
        })
        .catch(error => {
            console.error('Error fetching spare parts:', error);
            Swal.fire('Error', 'Failed to fetch spare parts. Please try again.', 'error');
        });
}

// إضافة قطعة جديدة
function addSaleCar() {
    const spareName = document.getElementById('spareName').value.trim();
    const spareImage = document.getElementById('sparepartsImage').files[0];
    const sparePrice = parseFloat(document.getElementById('sparepartsPrice').value.trim());
    const spareModel = document.getElementById('carModel').value.trim();
    const qty = parseInt(document.getElementById('qty').value.trim());

    if (!spareName || !spareImage || isNaN(sparePrice) || !spareModel || isNaN(qty)) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Please fill out all fields correctly!',
        });
        return;
    }

    const formData = new FormData();
    formData.append('SpareName', spareName);
    formData.append('SpareImage', spareImage);
    formData.append('SparePrice', sparePrice);
    formData.append('SpareModel', spareModel);
    formData.append('Qty', qty);

    fetch('http://localhost:5002/SpareParte/AddSparePart', {
        method: 'POST',
        body: formData,
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.text().then(text => text ? JSON.parse(text) : {});
        })
        .then(result => {
            Swal.fire({
                icon: 'success',
                title: 'Success',
                text: 'Spare part added successfully!',
            });
            fetchSpareParts();
        })
        .catch(error => {
            console.error('Error adding spare part:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to add spare part.',
            });
        });
}

function openEditModal(spare) {
    console.log('Editing spare part:', spare);
    document.getElementById('spareid').value = spare.spareId || '';
    document.getElementById('SpareName').value = spare.spareName || '';
    document.getElementById('spareModel').value = spare.spareModel || '';
    document.getElementById('sparePrice').value = spare.sparePrice || '';
    document.getElementById('Qty').value = spare.qty || '';

    const editModal = new bootstrap.Modal(document.getElementById('editModal'));
    editModal.show();
}

async function updateCar(event) {
    event.preventDefault();
    const spareId = parseInt(document.getElementById('spareid').value, 10);
    const spareName = document.getElementById('SpareName').value;
    const spareModel = document.getElementById('spareModel').value;
    const sparePrice = parseFloat(document.getElementById('sparePrice').value);
    const qty = parseInt(document.getElementById('Qty').value, 10);

    const spareData = { spareId, spareName, spareModel, sparePrice, qty };

    console.log('Data sent to API:', spareData);

    try {
        const response = await fetch(`http://localhost:5002/SpareParte/edit-car/${spareId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(spareData),
        });

        console.log('API Response:', response);

        if (response.ok) {
            Swal.fire('Success', 'Spare part updated successfully!', 'success');
            const editModal = bootstrap.Modal.getInstance(document.getElementById('editModal'));
            editModal.hide();
            fetchSpareParts();
        } else {
            const errorText = await response.text();
            console.error('API Error Response:', errorText);
            Swal.fire('Error', `Failed to update spare part: ${errorText}`, 'error');
        }
    } catch (error) {
        console.error('Error:', error);
        Swal.fire('Error', 'Something went wrong. Please try again.', 'error');
    }
}

async function deleteSparePart(spareId) {
    console.log('Deleting spare part with ID:', spareId);
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
            const response = await fetch(`http://localhost:5002/SpareParte/deleteSparePart/${spareId}`, {
                method: 'DELETE',
            });

            console.log('Response status:', response.status);
            const responseText = await response.text();
            console.log('Response body:', responseText);

            if (response.ok) {
                Swal.fire('Deleted!', 'The spare part has been deleted.', 'success');
                fetchSpareParts();
            } else {
                Swal.fire('Error', `Failed to delete spare part. Status: ${response.status}`, 'error');
            }
        } catch (error) {
            console.error('Error during deletion:', error);
            Swal.fire('Error', 'Failed to delete spare part due to a network or server issue.', 'error');
        }
    }
}

window.onload = fetchSpareParts;
document.getElementById('editModal').addEventListener('submit', updateCar);
