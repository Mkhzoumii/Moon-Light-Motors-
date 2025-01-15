function login(event) {
    event.preventDefault(); // منع إعادة تحميل الصفحة عند النقر على "Sign In"
    
    const email = document.querySelector('#email').value.trim();  // Get the email value
    const password = document.querySelector('#password').value.trim();  // Get the password value

    if (!email || !password) {
        alert('Please fill in both email and password fields.');
        return;
    }

    const LogInData = {
        email: email,
        password: password,
    };

    const url1 = 'http://localhost:5002/Auth/Login';  // Backend API for login

    fetch(url1, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(LogInData)
    })
    .then(response => {
        if (response.ok) {
            return response.json();
        } else if (response.status === 403) {
            // Check if the account is deactivated
            throw new Error('Account deactivated');
        } else {
            throw new Error('Invalid login credentials');
        }
    })
    .then(data => {
        if (data && data.token) {
            localStorage.setItem('authToken', data.token);
            localStorage.setItem('firstName', data.firstName);

            const token = localStorage.getItem("authToken");
            const url = "http://localhost:5002/Users/GetSingleUsers";

            return fetch(url, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    Authorization: `Bearer ${token}`,
                },
            });
        } else {
            throw new Error('Login failed. No token returned.');
        }
    })
    .then(response => {
        if (response.ok) {
            return response.json();
        } else {
            throw new Error("Failed to fetch user data.");
        }
    })
    .then(userData => {
        localStorage.setItem('userId', userData.userId);
        localStorage.setItem('is_admin', userData.is_admin);

        if (userData.is_admin === true) {
            window.location.href = 'admin12.html';
        } else {
            window.location.href = 'index.html';
        }
    })
    .catch(error => {
        console.error("Error:", error);

        if (error.message === 'Account deactivated') {
            // Show SweetAlert for deactivated account
            Swal.fire({
                icon: 'error',
                title: 'Account Deactivated',
                text: 'This account has been deactivated. Please contact support.',
            });
        } else {
            // Default error message
            Swal.fire({
                icon: 'error',
                title: 'Login Failed',
                text: 'Please check your credentials and try again.',
            });
        }
    });
}


function sendUserData(event) {
    event.preventDefault(); // منع إعادة تحميل الصفحة عند النقر على "Sign Up"
    
    const email = document.getElementById("Email").value.trim();
    const password = document.getElementById("pass").value.trim();
    const passwordConfirm = document.getElementById("confirmPass").value.trim();
    const firstName = document.getElementById("firstname").value.trim();
    const lastName = document.getElementById("lastname").value.trim();

    // التحقق من صحة الحقول
    if (!email || !password || !passwordConfirm || !firstName || !lastName) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Please fill in all fields.'
        });
        return;
    }

    if (password !== passwordConfirm) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: 'Passwords do not match.'
        });
        return;
    }

    // Create the data object
    const userData = {
        email: email,
        password: password,
        passwordConfirm: passwordConfirm,
        firstName: firstName,
        lastName: lastName
    };

    // Define the API endpoint
    const url = 'http://localhost:5002/Auth/Register';

    // Send the POST request
    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(userData)
    })
    .then(response => {
        // Check if the response is successful
        if (response.ok) {
            return response.json().catch(() => ({}));
        } else {
            return response.text().then(text => {
                throw new Error(text || 'Registration failed.');
            });
        }
    })
    .then(data => {
        console.log("Success:", data);

        // عرض رسالة نجاح
        Swal.fire({
            icon: 'success',
            title: 'Success',
            text: 'Registration successful! You can now log in.'
        });

        // إعادة تعيين الحقول
        document.getElementById("Email").value = '';
        document.getElementById("pass").value = '';
        document.getElementById("confirmPass").value = '';
        document.getElementById("firstname").value = '';
        document.getElementById("lastname").value = '';
    })
    .catch(error => {
        console.error("Error:", error);

        // عرض رسالة خطأ
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: error.message || 'Registration failed. Please try again.'
        });
    });
}

