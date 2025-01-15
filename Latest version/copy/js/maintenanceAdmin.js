function fetchMaintenanceData() {
    const tableBody = document.getElementById('maintenanceTableBody');
    if (!tableBody) {
        console.error("Table body with ID 'maintenanceTableBody' not found.");
        return;
    }

    fetch('http://localhost:5002/Maintenance/GetAllMaintenanceWithUserName')
        .then(response => response.json())
        .then(data => {
            tableBody.innerHTML = '';
            data.forEach(item => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${item.userID}</td>
                    <td>${item.firstName}</td>
                    <td>${new Date(item.start_date).toLocaleDateString()}</td>
                    <td>${new Date(item.end_date).toLocaleDateString()}</td>
                    <td>${item.maintenance_type}</td>
                    <td>${item.price}</td>
                    <td>
                        <button class="btn btn-primary btn-sm" onclick='openEditModal(${JSON.stringify(item)})'>Edit</button>
                        <button class="btn btn-danger btn-sm" onclick="deleteSparePart(${item.maintenance_id})">Delete</button>
                    </td>`;
                tableBody.appendChild(row);
            });
        })
        .catch(error => console.error('Error fetching maintenance data:', error));
}

function openEditModal(item) {
    document.getElementById('maintenanceType').value = item.maintenance_type;
    document.getElementById('startDate').value = new Date(item.start_date).toISOString().slice(0, -1);
    document.getElementById('endDate').value = new Date(item.end_date).toISOString().slice(0, -1);
    document.getElementById('userId').value = item.userID;
    document.getElementById('editModal').setAttribute('data-maintenance-id', item.maintenance_id);

    const modal = new bootstrap.Modal(document.getElementById('editModal'));
    modal.show();
}

async function saveChanges() {
    const maintenanceId = document.getElementById('editModal').getAttribute('data-maintenance-id');
    const maintenanceType = document.getElementById('maintenanceType').value.trim();
    const startDate = document.getElementById('startDate').value.trim();
    const endDate = document.getElementById('endDate').value.trim();
    const userId = document.getElementById('userId').value.trim();

    if (!maintenanceType || !startDate || !endDate) {
        Swal.fire('Error', 'All fields must be filled out.', 'error');
        return;
    }

    const updatedData = {
        maintenance_id: parseInt(maintenanceId, 10),
        maintenance_type: maintenanceType,
        userID: parseInt(userId, 10),
        start_date: new Date(startDate).toISOString(),
        end_date: new Date(endDate).toISOString(),
    };

    try {
        const response = await fetch(`http://localhost:5002/Maintenance/UpdateMaintenance/${maintenanceId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(updatedData),
        });

        if (response.ok) {
            Swal.fire('Success', 'Maintenance updated successfully.', 'success');
            fetchMaintenanceData();
        } else {
            const error = await response.json();
            Swal.fire('Error', error.message || 'Failed to update maintenance.', 'error');
        }
    } catch (error) {
        Swal.fire('Error', 'An unexpected error occurred.', 'error');
    }
}

async function deleteSparePart(spareId) {
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
            const response = await fetch(`http://localhost:5002/Maintenance/DeleteMaintenance/${spareId}`, {
                method: 'DELETE',
            });

            if (response.ok) {
                Swal.fire('Deleted!', 'The maintenance has been deleted.', 'success');
                fetchMaintenanceData();
            } else {
                Swal.fire('Error', 'Failed to delete maintenance.', 'error');
            }
        } catch {
            Swal.fire('Error', 'Failed to delete maintenance due to a network or server issue.', 'error');
        }
    }
}

// Load maintenance data on page load
window.onload = fetchMaintenanceData;
