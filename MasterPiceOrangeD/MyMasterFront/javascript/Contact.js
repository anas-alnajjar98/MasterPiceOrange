async function SendContact(event) {
    event.preventDefault();  

    try {
       
        const name = document.getElementById("name").value;
        const email = document.getElementById("email").value;
        const subject = document.getElementById("subject").value;
        const message = document.getElementById("message").value;
        const UserId = localStorage.getItem("userId");

       
        const userData = {
            Name: name,
            Email: email,
            Subject: subject,
            Message: message,
            UserId: UserId
        };

        
        const response = await fetch(
            "https://localhost:7260/api/User/Submit",
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(userData), 
            }
        );

        
        if (response.ok) {
            Swal.fire({
                icon: 'success',
                title: 'Message Sent',
                text: 'Your contact message has been sent successfully!',
                timer: 3000,
                showConfirmButton: false
            });

           
            document.getElementById("name").value = "";
            document.getElementById("email").value = "";
            document.getElementById("subject").value = "";
            document.getElementById("message").value = "";
        } else {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to send the message. Please try again later.',
            });
        }
    } catch (e) {
        console.error("Error during message sending:", e);
        Swal.fire({
            icon: 'error',
            title: 'Exception',
            text: 'An unexpected error occurred. Please try again later.',
        });
    }
}
