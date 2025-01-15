    // التحقق من وجود اسم المستخدم في localStorage
      window.onload = function() {
        const userName = localStorage.getItem("firstName");
        const userGreeting = document.querySelector(".nav-item .dropdown-toggle");

        if (userName) {
            // إذا كان يوجد اسم مستخدم، عرض اسمه في الـ nav
            userGreeting.textContent = `Welcome, ${userName}`;
            // إخفاء زر "Login"
            // إظهار زر "Edit profile" و "Cart"
            document.getElementById("editProfileLink").style.display = "block";
            document.getElementById("cartLink").style.display = "block";
        } else {
            // إذا لم يكن هناك اسم مستخدم، إظهار زر "Login" فقط
            // إخفاء زر "Edit profile" و "Cart"
            document.getElementById("editProfileLink").style.display = "none";
            document.getElementById("cartLink").style.display = "none";
        }
    }
     
      const token = localStorage.getItem("authToken");
      console.log(token); 
      if(token==null)
      {
        document.getElementById("login").style.display = "inline-block";
        document.getElementById("search").style.display = "none";
        


      }
      else{
        document.getElementById("login").style.display = "none";
        document.getElementById("search").style.display = "block";
       
      }

