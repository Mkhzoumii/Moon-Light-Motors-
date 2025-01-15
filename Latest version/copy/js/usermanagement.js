document.addEventListener("DOMContentLoaded", function () {
    const apiUrl = 'http://localhost:5002/Users/GetUsers';

    fetch(apiUrl, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${localStorage.getItem('authToken')}`
        }
    })
        .then(response => response.json())
        .then(users => {
            const tableBody = document.querySelector('.table tbody');
            tableBody.innerHTML = '';

            users.forEach(user => {
                const row = document.createElement('tr');

                row.innerHTML = `
                    <td>${user.userID}</td>
                    <td>${user.firstName}</td>
                    <td>${user.lastName}</td>
                    <td>${user.email}</td>
                    <td>${user.is_active ? 'Active' : 'Deactivated'}</td>
                    <td>
                        <button class="btn btn-primary btn-sm edit-btn" data-id="${user.userID}">Edit</button>
                        <button class="btn btn-danger btn-sm delete-btn" data-id="${user.userID}">Delete</button>
                    </td>
                `;

                if (!user.is_active) row.style.backgroundColor = '#f8d7da';

                tableBody.appendChild(row);
            });

            document.querySelectorAll('.edit-btn').forEach(button => {
                button.addEventListener('click', function () {
                    const userId = this.getAttribute('data-id'); // الحصول على userId من الجدول
                    openEditModal(userId);
                });
            });

            document.querySelectorAll('.delete-btn').forEach(button => {
                button.addEventListener('click', function () {
                    const userId = this.getAttribute('data-id');
                    deactivateUser(userId);
                });
            });
        })
        .catch(error => {
            console.error('Error:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error Fetching Users',
                text: error.message,
            });
        });
});

function openEditModal(userId) {
    const apiUrl = `http://localhost:5002/Users/GetSingleUser/${userId}`; // تعديل نقطة النهاية

    fetch(apiUrl, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${localStorage.getItem('authToken')}`
        }
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                throw new Error('Failed to fetch user data.');
            }
        })
        .then(user => {
            console.log(user); // عرض استجابة الخادم للتحقق من القيم

            // تعبئة الحقول في المودال مع معالجة القيم الفارغة
            document.getElementById('userId').value = user.userID || '';
            document.getElementById('username').value = user.firstName || ''; // اسم افتراضي فارغ
            document.getElementById('email').value = user.email || ''; // بريد إلكتروني افتراضي فارغ

            // فتح المودال
            const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('editUserModal'));
            modal.show();
        })
        .catch(error => {
            console.error('Error:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to load user data.',
            });
        });
}

document.getElementById('saveChangesBtn').addEventListener('click', function () {
    const userId = document.getElementById('userId').value;
    const username = document.getElementById('username').value.trim();
    const email = document.getElementById('email').value.trim();

    if (!username && !email) {
        Swal.fire({
            icon: 'error',
            title: 'Validation Error',
            text: 'Please enter at least one field to update.',
        });
        return;
    }

    const token = localStorage.getItem('authToken');
    const requestBody = {
        userId: userId,
        newFirstName: username,
        newEmail: email
    };

    fetch('http://localhost:5002/Auth/UpdateUserDetails', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`
        },
        body: JSON.stringify(requestBody)
    })
        .then(response => {
            if (response.ok) {
                Swal.fire({
                    icon: 'success',
                    title: 'Success',
                    text: 'User updated successfully!',
                });
                const modal = bootstrap.Modal.getInstance(document.getElementById('editUserModal'));
                modal.hide();
                document.dispatchEvent(new Event("DOMContentLoaded"));
            } else {
                return response.text().then(text => {
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: text,
                    });
                });
            }
        })
        .catch(error => {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to update user data.',
            });
            console.error('Error:', error);
        });
});

function deactivateUser(userId) {
    const apiUrl = `http://localhost:5002/Users/DeactivateUser/${userId}`;

    fetch(apiUrl, {
        method: 'DELETE',
        headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${localStorage.getItem('authToken')}`
        }
    })
        .then(response => {
            if (response.ok) {
                Swal.fire({
                    icon: 'success',
                    title: 'User Deactivated',
                    text: 'The user has been successfully deactivated.',
                });
                document.dispatchEvent(new Event("DOMContentLoaded"));
            } else {
                throw new Error('Failed to deactivate user.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to deactivate user.',
            });
        });
}
