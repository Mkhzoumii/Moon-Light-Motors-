function isAuthenticated() {
  // Check if a token exists in localStorage
  const token = localStorage.getItem("authToken");
  return token !== null; // Returns true if token exists, false otherwise
}

function logout() {
  // Remove the auth token from localStorage
  localStorage.removeItem("authToken");
  localStorage.removeItem("userId");
  localStorage.removeItem("firstName");
  localStorage.removeItem("is_admin");
  // Redirect to login page
  window.location.href = "loginabood.html";
}

const tabs = document.querySelectorAll("ul.list-unstyled li");
const cardsContainer = document.getElementById('cards-container');
const buttonsContainer = document.getElementById('buttons-container');

function generateCards(categoryId) {
  const url = categoryId === 'spare-parts' 
    ? "http://localhost:5002/SpareParte/GetSpareParte" // قطع الغيار
    : "http://localhost:5002/Cars/GetAvailableCars"; // السيارات

  fetch(url)
    .then((res) => res.json())
    .then((data) => {
      cardsContainer.innerHTML = ''; // مسح المحتوى القديم

      // عرض فقط 4 عناصر للسيارات أو القطع
      const firstItems = data.slice(0, 4);

      // حالياً بناء على الفئة إما عرض قطع الغيار أو السيارات
      firstItems.forEach(item => {
        const cardHTML = categoryId === 'spare-parts' 
          ? getSparePartCardHTML(item) 
          : getCarCardHTML(item);
          
        cardsContainer.insertAdjacentHTML('beforeend', cardHTML);

        // استدعاء دالة جلب الصورة الأولى
        if (categoryId !== 'spare-parts') {
          fetchFirstImageIdForSale(item.carID, `img-${item.carID}`);
        }
      });
      
    })
    .catch(error => {
      console.error("Error fetching data:", error);
      cardsContainer.innerHTML = '<p class="text-center text-danger">Error loading products.</p>';
    });

  generateButton(categoryId); // عرض زر التنقل الصحيح بناءً على الفئة
}

function getCarCardHTML(car) {
  return `
    <div class="col-sm-6 col-md-4 col-lg-3 box">
      <div class="card">
        <div class="image-container" style="position: relative; overflow: hidden; height: 200px;">
          <img id="img-${car.carID}" class="card-img-top" style="width: 100%; height: 100%; object-fit: cover;" alt="${car.carName}">
        </div>
        <div class="card-body pt-3 px-0">
          <div class="car-name mb-0 mt-0 px-3"><h4>${car.carName}</h4></div>
          <div class="d-flex justify-content-between mb-0 px-3">
            <small class="text-muted mt-1">Price </small>
            <h6><span>$</span>${car.carPrice}</h6>
          </div>
          <hr class="mt-2 mx-3">
          <div class="d-flex justify-content-between px-3 pb-4">
                                  <small class="text-muted mt-1">Model </small>

            <div class="d-flex flex-column">

              <h5 class="mb-0">${car.carModel}</h5>
            </div>
          </div>
         
          <div class="mx-3 mt-3 mb-2">
            <button type="button" class="btn btn-danger btn-block" onclick="openDetails(${car.carID}, 'sale',${car.carPrice})">
              <small>Details</small>
            </button>
          </div>
        </div>
      </div>
    </div>`;
}

function getSparePartCardHTML(sparePart) {
  return `
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
            <button type="button" class="btn btn-danger btn-block" onclick="EditSparePart(${sparePart.spareId})">
              <small>EDIT</small>
            </button>
          </div>
        </div>
      </div>
    </div>`;
}

function generateButton(categoryId) {
  buttonsContainer.innerHTML = ''; // مسح الأزرار القديمة

  let targetPage = '';
  let buttonText = '';

  if (categoryId === 'sales') {
    targetPage = 'sales.html';
    buttonText = 'Browse Sales';
  } else if (categoryId === 'rentals') {
    targetPage = 'Rentals.html';
    buttonText = 'Browse Rentals';
  } else if (categoryId === 'spare-parts') {
    targetPage = 'SpareParts.html';
    buttonText = 'Browse Spare Parts';
  } else {
    console.error('Invalid categoryId:', categoryId);
    return;
  }

  // إنشاء الزر بناءً على الفئة
  const buttonHTML = `
    <a href="${targetPage}" class="btn btn-secondary rounded-pill p-3 workout-btn">
      ${buttonText}
    </a>`;

  // إضافة الزر إلى الحاوية
  buttonsContainer.insertAdjacentHTML('beforeend', buttonHTML);
}

function openDetails(carId, source, carPrice) {
  window.open(`details.html?data=${encodeURIComponent(carId)}&source=${source}&price=${encodeURIComponent(carPrice)}`, "_blank");
}

// Initialize the page by loading cars (or spare parts)
generateCards('vehicles');

// Event listener for tabs
tabs.forEach(tab => {
  tab.addEventListener('click', function () {
    tabs.forEach(t => t.classList.remove('active')); 
    this.classList.add('active'); 

    const categoryId = this.id;
    generateCards(categoryId); // استدعاء دالة generateCards بناءً على الفئة
  });
});

function RentCar() {
  const url = "http://localhost:5002/CarsForRent/GetCars";

  fetch(url)
    .then((res) => res.json())
    .then((data) => {
      // مسح المحتوى القديم
      const cardsContainer = document.getElementById("cards-container");
      cardsContainer.innerHTML = '';

      // أخذ أول 4 سيارات فقط
      const firstForCars = data.slice(0, 4);

      // إنشاء البطاقات
      firstForCars.forEach((car) => {
        const cardHTML = `
          <div class="col-sm-6 col-md-4 col-lg-3 box">
            <div class="card">
              <div class="image-container" style="position: relative; overflow: hidden; height: 200px;">
                <img id="img-${car.carid}" class="card-img-top" style="width: 100%; height: 100%; object-fit: cover;" alt="${car.carname}">
              </div>
              <div class="card-body pt-3 px-0">
                <div class="car-name mb-0 mt-0 px-3"><h4>${car.carname}</h4></div>
                <div class="d-flex justify-content-between mb-0 px-3">
                  <small class="text-muted mt-1">STARTING AT</small>
                  <h6>${car.price}</h6>
                </div>
                <hr class="mt-2 mx-3">
                <div class="d-flex justify-content-between px-3 pb-4">
                  <div class="d-flex flex-column">
                    <h5 class="mb-0">${car.model}</h5>
                  </div>
                </div>
                <div class="mx-3 mt-3 mb-2">
                  <button type="button" class="btn btn-danger btn-block">
                    <small>BUILD & PRICE</small>
                  </button>
                </div>
                <div class="mx-3 mt-3 mb-2">
                  <button type="button" class="btn btn-danger btn-block" onclick="openDetails(${car.carid}, 'rent', ${car.price})">
                    <small>Details</small>
                  </button>
                </div>
              </div>
            </div>
          </div>`;

        cardsContainer.insertAdjacentHTML('beforeend', cardHTML);

        // جلب الصورة الأولى لكل سيارة
        fetchFirstImageId(car.carid, `img-${car.carid}`);
      });
    })
    .catch((error) => {
      console.error("Error fetching cars:", error);
    });
}


// وظيفة جلب الصورة الأولى لكل سيارة للبيع
function fetchFirstImageIdForSale(carId, imgElementId) {
  fetch(`http://localhost:5002/Cars/get-images-by-carid/${carId}`)
    .then((res) => res.json())
    .then((data) => {
      const imageIds = data.imageIds;
      if (imageIds && imageIds.length > 0) {
        const firstImageId = imageIds[0];
        loadImageforSale(firstImageId, imgElementId);
      } else {
        document.getElementById(imgElementId).src = 'path/to/default/image.jpg'; // صورة افتراضية إذا لم توجد صور
      }
    })
    .catch((error) => {
      console.error("Error fetching image IDs:", error);
      document.getElementById(imgElementId).src = 'path/to/default/image.jpg'; // صورة افتراضية
    });
}

// وظيفة جلب الصورة الأولى لكل سيارة للإيجار
function fetchFirstImageId(carId, imgElementId) {
  console.log("Fetching image IDs for car ID:", carId);

  fetch(`http://localhost:5002/CarsForRent/get-images-by-carid/${carId}`)
    .then((res) => res.json())
    .then((data) => {
      const imageIds = data.imageIds;
      console.log("Image IDs received:", imageIds);

      if (imageIds && imageIds.length > 0) {
        const firstImageId = imageIds[0];
        loadImage(firstImageId, imgElementId);
      } else {
        const imgElement = document.getElementById(imgElementId);
        if (imgElement) {
          imgElement.src = 'path/to/default/image.jpg'; // صورة افتراضية
        } else {
          console.error(`Element with ID ${imgElementId} not found`);
        }
      }
    })
    .catch((error) => {
      console.error("Error fetching image IDs:", error);
      const imgElement = document.getElementById(imgElementId);
      if (imgElement) {
        imgElement.src = 'path/to/default/image.jpg'; // صورة افتراضية عند الخطأ
      }
    });
}



// وظيفة لتحميل الصورة من خلال ID
function loadImage(imageId, imgElementId) {
  console.log("Loading Image ID:", imageId, "Image Element ID:", imgElementId);

  const imgElement = document.getElementById(imgElementId);
  if (!imgElement) {
    console.error(`Element with ID ${imgElementId} not found`);
    return; // إنهاء التنفيذ إذا لم يوجد العنصر
  }

  fetch(`http://localhost:5002/CarsForRent/get-image-by-id/${imageId}`)
    .then((res) => {
      if (!res.ok) {
        throw new Error("Failed to load image");
      }
      return res.blob();
    })
    .then((blob) => {
      const imageUrl = URL.createObjectURL(blob);
      console.log("Image URL:", imageUrl);

      imgElement.src = imageUrl;
    })
    .catch((error) => {
      console.error("Error loading image:", error);
      imgElement.src = 'path/to/default/image.jpg'; // صورة افتراضية
    });
}




function loadImageforSale(imageId, imgElementId) {
  fetch(`http://localhost:5002/Cars/get-image-by-id/${imageId}`)
    .then((res) => {
      if (!res.ok) {
        throw new Error("Failed to load image");
      }
      return res.blob();
    })
    .then((blob) => {
      const imageUrl = URL.createObjectURL(blob);
      document.getElementById(imgElementId).src = imageUrl;
    })
    .catch((error) => {
      console.error("Error loading image:", error);
    });
}

async function fetchAndRenderAllSpareParts() {
  try {
      const url = "http://localhost:5002/SpareParte/GetSpareParte";
      const response = await fetch(url);
      const data = await response.json();

      // حفظ البيانات في متغير عام لاستخدامها في البحث والترتيب
      window.sparePartsData = data;

      // عرض جميع القطع عند التحميل
      renderSpareParts(data);
  } catch (error) {
      console.error("Error fetching SpareParte data:", error);
      document.getElementById("cards-container").innerHTML =
          '<p class="text-center text-danger">Error loading spare parts.</p>';
  }
}

function renderSpareParts() {
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
                        <button type="button" class="btn btn-danger btn-block" onclick="EditSparePart(${sparePart.spareId})">
                            <small>EDIT</small>
                        </button>
                    </div>
                </div>
            </div>
        </div>`;
    cardsContainer.insertAdjacentHTML("beforeend", cardHTML);
  });
}