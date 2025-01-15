async function fetchAndRenderAllSpareParts() {
  try {
    const url = "http://localhost:5002/SpareParte/GetSpareParte";
    const response = await fetch(url);
    const data = await response.json();

    // حفظ البيانات في متغير عام لاستخدامها في البحث والترتيب
    window.sparePartsData = data;
    console.log(data);

    // عرض جميع القطع عند التحميل
    renderSpareParts(data);
  } catch (error) {
    console.error("Error fetching SpareParte data:", error);
    document.getElementById("cards-container").innerHTML =
      '<p class="text-center text-danger">Error loading spare parts.</p>';
  }
}

// عرض البيانات في الصفحة
function renderSpareParts(spareParts) {
  const cardsContainer = document.getElementById("cards-container");
  cardsContainer.innerHTML = "";

  // التحقق إذا لم يكن هناك أي قطع
  if (spareParts.length === 0) {
    cardsContainer.innerHTML = '<p class="text-center text-danger">No spare parts available.</p>';
    return;
  }

  // إنشاء الكروت لكل قطعة
  spareParts.forEach((sparePart) => {
    const cardHTML = `
      <div class="col-sm-6 col-md-4 col-lg-3 box">
        <div class="card">
          <img src="http://localhost:5002/SpareParte/GetSpareImage/${sparePart.spareId}" class="card-img-top" alt="${sparePart.spareName}">
          <div class="card-body pt-3 px-0">
            <div class="car-name mb-0 mt-0 px-3">
              <h4>${sparePart.spareName}</h4>
            </div>
            <div class="d-flex justify-content-between mb-0 px-3">
              <small class="text-muted mt-1">PRICE</small>
              <h6>$${sparePart.sparePrice}</h6>
            </div>
            <hr class="mt-2 mx-3">
            <div class="d-flex justify-content-between px-3 pb-4">
              <div class="d-flex flex-column">
                <small class="text-muted mt-1">Spare Model</small>
                <h5 class="mb-0">${sparePart.spareModel}</h5>
              </div>
            </div>
            <div class="d-flex justify-content-between px-3 pb-4">
              <div class="d-flex flex-column">
                <small class="text-muted mt-1">Quantity</small>
                <h5 class="mb-0">${sparePart.qty}</h5>
              </div>
            </div>
            <div class="mx-3 mt-3 mb-2">
              <button type="button" class="btn btn-danger btn-block" onclick="OpenBasket(${sparePart.spareId}, ${sparePart.sparePrice}, '${sparePart.spareName}', ${userId}, ${sparePart.qty})">
                <small>Add To Basket</small>
              </button>
            </div>
          </div>
        </div>
      </div>
    `;
    cardsContainer.insertAdjacentHTML("beforeend", cardHTML);
  });
}

// البحث عن قطعة معينة باستخدام spareName و spareModel
function searchSpareParts(query) {
  const filteredParts = window.sparePartsData.filter((sparePart) =>
    sparePart.spareName.toLowerCase().includes(query.toLowerCase()) ||
    sparePart.spareModel.toLowerCase().includes(query.toLowerCase())
  );
  renderSpareParts(filteredParts);
}

// ترتيب القطع حسب السعر
function sortSpareParts(order) {
  const sortedParts = [...window.sparePartsData].sort((a, b) => {
    return order === "lowToHigh"
      ? a.sparePrice - b.sparePrice
      : b.sparePrice - a.sparePrice;
  });
  renderSpareParts(sortedParts);
}

// تحميل كل القطع عند فتح الصفحة
window.onload = () => {
  fetchAndRenderAllSpareParts();

  // إضافة حدث البحث
  const searchInput = document.getElementById("carSearch");
  searchInput.addEventListener("input", (event) => {
    const query = event.target.value.trim();
    if (query.length >= 2) {
      searchSpareParts(query); // البحث
    } else {
      renderSpareParts(window.sparePartsData); // عرض جميع القطع
    }
  });

  // إضافة حدث الترتيب
  const sortOptions = document.querySelectorAll(".dropdown-item");
  sortOptions.forEach((option) => {
    option.addEventListener("click", (event) => {
      const sortType = event.target.textContent.trim();
      if (sortType === "Low to high") {
        sortSpareParts("lowToHigh");
      } else if (sortType === "High to low") {
        sortSpareParts("highToLow");
      }
    });
  });
};

const userId = localStorage.getItem("userId");
if (!userId) {
  console.error("User ID not found in localStorage.");
}

// فتح السلة
// دالة لإضافة القطعة إلى السلة
function OpenBasket(spareId, sparePrice, spareName, userId, qty) {
  // إعداد البيانات التي سيتم إرسالها
  const basketData = {
    spareId: spareId,
    sparePrice: sparePrice,
    sparePartName: spareName,
    qty: qty, // عدد القطعة (يمكنك تعديله بناءً على الحاجة)
    userId: userId
  };

  // إرسال البيانات إلى الـ API باستخدام fetch
  fetch("http://localhost:5002/SpareParte/AddToBasket", {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(basketData)  // تحويل البيانات إلى JSON
  })
    .then(response => {
      if (response.ok) {
        // إذا كانت الاستجابة ناجحة، عرض رسالة SweetAlert
        Swal.fire({
          icon: 'success',
          title: 'Item Added!',
          text: `${spareName} has been added to your basket.`,
          confirmButtonText: 'Okay'
        });
      } else {
        // إذا حدث خطأ في إضافة القطعة
        Swal.fire({
          icon: 'error',
          title: 'Failed to Add!',
          text: 'There was an error adding the item to the basket.',
          confirmButtonText: 'Try Again'
        });
      }
    })
    .catch(error => {
      console.error("Error adding item to basket:", error);
      Swal.fire({
        icon: 'error',
        title: 'Error!',
        text: 'An error occurred while adding the item to the basket.',
        confirmButtonText: 'Try Again'
      });
    });
}

function gotocart() {
  const userId = localStorage.getItem("userId");
  console.log(userId);  

  if (userId) {
    window.location.href = `cart.html?userId=${userId}`;
    // توجيه المستخدم إلى صفحة السلة مع تمرير userId في الرابط
  } else {
    alert("You must be logged in to complete the purchase.");
  }}
// دالة لفتح صفحة إتمام الشراء (السلة)
