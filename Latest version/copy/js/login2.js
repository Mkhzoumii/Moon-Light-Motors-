const container = document.getElementById('container');
const registerBtn = document.getElementById('register');
const loginBtn = document.getElementById('login');

registerBtn.addEventListener('click', () => {
    container.classList.add("active");
});

loginBtn.addEventListener('click', () => {
    container.classList.remove("active");
});


const container1 = document.querySelector('.container1');
const container2 = document.querySelector('.container2');
const toSignUpLink = document.querySelector('.toSignUp');
const toSignInLink = document.querySelector('.toSignIn');

toSignUpLink.addEventListener('click', (event) => {
    event.preventDefault(); 
    container1.classList.add('d-sm-none'); 
    container1.classList.remove('d-sm-block'); 
    container2.classList.add('d-sm-block'); 
    container2.classList.remove('d-sm-none')
});

toSignInLink.addEventListener('click', (event) => {
    event.preventDefault(); 
    container2.classList.add('d-sm-none'); 
    container2.classList.remove('d-sm-block'); 
    container1.classList.add('d-sm-block'); 
    container1.classList.remove('d-sm-none')
});