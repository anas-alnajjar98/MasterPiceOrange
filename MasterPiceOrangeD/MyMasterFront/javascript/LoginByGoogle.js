import { initializeApp } from "https://www.gstatic.com/firebasejs/10.13.1/firebase-app.js";
import {
  getAuth,
  GoogleAuthProvider,
  signInWithPopup,
} from "https://www.gstatic.com/firebasejs/10.13.1/firebase-auth.js";

const firebaseConfig = {
  apiKey: "AIzaSyAv5vCoLdqubm_IJAOjNlgF7o9zo-1-VfE",
  authDomain: "login-3e8e4.firebaseapp.com",
  projectId: "login-3e8e4",
  storageBucket: "login-3e8e4.appspot.com",
  messagingSenderId: "251369161445",
  appId: "1:251369161445:web:ef0c157a6b0cdcdb1a0a0c",
};
const app = initializeApp(firebaseConfig);
const auth = getAuth(app);
auth.languageCode = "en";
const provider = new GoogleAuthProvider();

const googleLogin = document.getElementById("google-login-btn");

if (googleLogin) {
  googleLogin.addEventListener("click", async function () {
    debugger;
    try {
      const result = await signInWithPopup(auth, provider);

      const user = result.user;

      const { uid, displayName, email, photoURL } = user;
      const [firstName, lastName] = displayName
        ? displayName.split(" ")
        : ["", ""];

      // Save user details to localStorage
      localStorage.setItem(
        "user",
        JSON.stringify({
          uid,
          displayName,
          email,
          photoURL,
        })
      );

      // Prepare user data for API request
      const userData = {
        Email: email,
        Password: uid, // using uid as password
      };

      // Send user data to the API
      const response = await fetch(
        "https://localhost:7260/api/User/Login",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(userData), 
        }
      );

      
      if (!response.ok) {
        const errorText = await response.text();
        console.error(
          `HTTP error! Status: ${response.status}, Message: ${errorText}`
        );
        throw new Error(`HTTP error! Status: ${response.status}`);
      }

      
      const data = await response.json();
console.log(data);
     
      localStorage.setItem("Token", data.token);
      localStorage.setItem("userId", data.userId);
      localStorage.messeges = JSON.stringify([
        {
          title: "Login Successful",
          message: "You have been logged in successfully",
        },
      ]);

  
      Swal.fire({
        icon: "success",
        title: "Login Successful!",
        text: "You have been logged in successfully. Redirecting...",
        showConfirmButton: false,
        timer: 2000
      }).then(() => {
        
        window.location.href = "index.html";
      });
    } catch (error) {
      
      Swal.fire({
        icon: "error",
        title: "Login Error!",
        text: "Error during login. Please try again.",
        confirmButtonText: "OK"
      });
      console.error("Error during login or API request:", error);
    }
  });
} else {
  console.error("Google login button not found");
}
