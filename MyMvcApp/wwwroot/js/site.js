<script>
    document.getElementById('fetchUsersBtn').onclick = function () {
        fetch('https://localhost:7039/api/Registration/GetAllRegistrations') // Ensure this URL is correct
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                // Check if 'modifiedID' exists and is an array
                if (data.modifiedID && Array.isArray(data.modifiedID)) {
                    const usersListDiv = document.getElementById('usersList');
                    usersListDiv.innerHTML = ""; // Clear previous data

                    // Use a traditional for loop to iterate through the users
                    for (let i = 0; i < data.modifiedID.length; i++) {
                        const user = data.modifiedID[i]; // Access each user
                        usersListDiv.innerHTML += `<p>${user.empName} - ${user.empEmail}</p>`; // Display user details
                    }
                } else {
                    console.error("Unexpected response format:", data);
                    alert("No user data available.");
                }
            })
            .catch(error => {
                console.error('There has been a problem with your fetch operation:', error);
            });
    };
</script>
