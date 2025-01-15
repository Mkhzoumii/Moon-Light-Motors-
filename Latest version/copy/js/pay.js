document.querySelector(".btn").addEventListener("click", function () {
    // Get input values using placeholders to target elements
    const cardHolder = document
      .querySelector('input[placeholder="Coding Market"]')
      .value.trim();
    const cardNumber = document
      .querySelector('input[placeholder="Card Number"]')
      .value.trim();
    const expiryDate = document
      .querySelector('input[placeholder="00 / 00"]')
      .value.trim();
    const cvc = document.querySelector('input[placeholder="000"]').value.trim();
  
    // Regular expressions for validation
    const cardNumberRegex = /^[0-9]{13,19}$/; // Allow 13 to 19 digits
    const expiryDateRegex = /^(0[1-9]|1[0-2]) \/ [0-9]{2}$/; // MM / YY
    const cvcRegex = /^[0-9]{3}$/; // 3 digits
  
    // Validate fields
    if (!cardHolder) {
      alert("Card holder name is required.");
      return;
    }
  
    if (!cardNumberRegex.test(cardNumber.replace(/\s+/g, ""))) {
      alert("Card number must be between 13 and 19 digits without spaces.");
      return;
    }
  
    if (!isValidExpiryDate(expiryDate)) {
      alert(
        "Invalid expiry date. Ensure it's in MM / YY format and not expired."
      );
      return;
    }
  
    if (!cvcRegex.test(cvc)) {
      alert("CVC must be a 3-digit number.");
      return;
    }
  
    // If all checks pass, proceed with the API request
    addPaymentCard(cardHolder, cardNumber.replace(/\s+/g, ""), expiryDate, cvc);
  });
  
  function isValidExpiryDate(expiryDate) {
    // Regular expression to check MM / YY format
    const expiryRegex = /^(0[1-9]|1[0-2]) \/ \d{2}$/;
  
    // Check if the format is valid
    if (!expiryRegex.test(expiryDate)) {
      return false; // Invalid format
    }
  
    // Extract month and year
    const [month, year] = expiryDate.split(" / ").map(Number);
  
    // Get current date and year
    const currentDate = new Date();
    const currentMonth = currentDate.getMonth() + 1; // Months are 0-indexed
    const currentYear = currentDate.getFullYear() % 100; // Get last two digits of the year
  
    // Check if the expiry date is in the future
    if (year < currentYear || (year === currentYear && month < currentMonth)) {
      return false; // Expiry date is in the past
    }
  
    return true; // Expiry date is valid
  }