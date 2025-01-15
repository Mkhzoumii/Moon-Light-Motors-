document.getElementById("updatePasswordForm").addEventListener("submit", function(event) {
    event.preventDefault(); // Prevent form submission from reloading the page

    // Get the values from the form inside the modal
    const currentPassword = document.getElementById("currentPassword").value;
    const newPassword = document.getElementById("newPassword").value;
    const confirmNewPassword = document.getElementById("confirmNewPassword").value;

    // Validate the passwords
    if (newPassword !== confirmNewPassword) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'New password and confirmation do not match.',
        });
        return;
    }

    // Get the token from localStorage (assuming the user is logged in)
    const token = localStorage.getItem('authToken');
    if (!token) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'You must be logged in to update your password.',
        });
        return;
    }

    // Prepare data for API request
    const updateData = {
        currentPassword: currentPassword,
        newPassword: newPassword,
    };

    const url = 'http://localhost:5002/Auth/UpdatePassword';  // API endpoint for updating password

    // Send the password update request
    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`, // Include the token in the Authorization header
        },
        body: JSON.stringify(updateData),  // Send the updated password data in the request body
    })
    .then(response => {
        if (!response.ok) {
            return response.text().then(text => {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: `Error updating password: ${text}`,
                });
                throw new Error(`Error response: ${text}`);
            });
        }
        // Attempt to parse JSON response
        return response.json().catch(() => {
            // If response is not JSON, treat it as success
            return { success: true };
        });
    })
    .then(data => {
        if (data.success || data) {
            Swal.fire({
                icon: 'success',
                title: 'Success',
                text: 'Password updated successfully!',
            });
            document.getElementById("updatePasswordForm").reset();

            // Close the modal programmatically
            const modal = bootstrap.Modal.getInstance(document.getElementById("passwordModal"));
            modal.hide();
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Unexpected response format.',
            });
            console.error("Unexpected response data:", data);
        }
    })
    .catch(error => {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'An error occurred while updating the password. Please try again.',
        });
        console.error('Error:', error);
    });
});
// إضافة مستمع لحدث الإرسال للنموذج
// منع إعادة تحميل الصفحة
function EditProfile() {
    // جلب القيم المدخلة
    const email = document.getElementById("email").value;
    const username = document.getElementById("username").value;

    // التحقق من وجود التوكين
    const token = localStorage.getItem("authToken");
    const userId = localStorage.getItem("userId");
    if (!token) {
        Swal.fire({
            icon: 'error',
            title: 'Unauthorized',
            text: 'You must be logged in to update your profile.',
        });
        return;
    }

    // التحقق من الحقول المدخلة
    if (username.trim() === "" && email.trim() === "") {
        Swal.fire({
            icon: 'error',
            title: 'No Input',
            text: 'Please fill at least one field to update.',
        });
        return;
    }

    // إذا كان هناك مدخل لاسم المستخدم والبريد الإلكتروني معًا
    if (username.trim() !== "" && email.trim() !== "") {
        if (!email.includes("@")) {
            Swal.fire({
                icon: 'error',
                title: 'Invalid Email',
                text: 'Please enter a valid email address.',
            });
            return;
        }

        fetch('http://localhost:5002/Auth/UpdateUserDetails', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({ newFirstName: username, newEmail: email, userId: userId })
        })
            .then(response => {
                if (response.ok) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: 'Profile updated successfully!',
                    });
                    document.getElementById("editProfileForm").reset(); // إعادة تعيين النموذج
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
                    text: 'Failed to update profile. Please try again.',
                });
                console.error('Error:', error);
            });
        return;
    }

    // إذا كان هناك مدخل لاسم المستخدم فقط
    if (username.trim() !== "" && email.trim() === "") {
        fetch('http://localhost:5002/Auth/UpdateFirstName', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({ newFirstName: username, userId: userId })
        })
            .then(response => {
                if (response.ok) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: 'Username updated successfully!',
                    });
                    document.getElementById("editProfileForm").reset(); // إعادة تعيين النموذج
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
                    text: 'Failed to update username. Please try again.',
                });
                console.error('Error:', error);
            });
    }

    // إذا كان هناك مدخل للإيميل فقط
    if (email.trim() !== "" && username.trim() === "") {
        if (!email.includes("@")) {
            Swal.fire({
                icon: 'error',
                title: 'Invalid Email',
                text: 'Please enter a valid email address.',
            });
            return;
        }

        fetch('http://localhost:5002/Auth/UpdateEmail', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({ newEmail: email, userId: userId })
        })
            .then(response => {
                if (response.ok) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Success',
                        text: 'Email updated successfully!',
                    });
                    document.getElementById("editProfileForm").reset(); // إعادة تعيين النموذج
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
                    text: 'Failed to update email. Please try again.',
                });
                console.error('Error:', error);
            });
    }
}
