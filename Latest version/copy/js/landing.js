// landing IDs
let bg = document.getElementById("bg");
let moon = document.getElementById("moon");
let mountain = document.getElementById("mountain");
let road = document.getElementById("road");
let text = document.getElementById("text");
// project acIDs
let projectP = document.getElementById("project-p");
let projectA = document.getElementById("project-a");

window.addEventListener("scroll", function () {
  let value = window.scrollY;
  bg.style.top = value * 0.5 + "px";
  moon.style.right = value * 0.5 + "px";
  mountain.style.top = value * 0 + "px";
  road.style.top = value * 0.15 + "px";
  text.style.top = value * 1 + "px";
});

// Get the project-h1 element
const projectH1 = document.getElementById("project-h1");

// Add a scroll event listener to the window
window.addEventListener("scroll", () => {
  let scrollValue = window.scrollY;
  projectH1.style.transform = `translateY(-${scrollValue * 0.2}px)`;
  projectH1.style.opacity = 1 - scrollValue / 50000000000;

  projectP.style.transform = `translateY(-${scrollValue * 0.15}px)`;
  projectP.style.opacity = 1 - scrollValue / 1000;

  projectA.style.transform = `translateY(-${scrollValue * 0.1}px)`;
  projectA.style.opacity = 1 - scrollValue / 1000;
});

// feature
const mainTitle = document.getElementById("features_main-title");
const feat = document.getElementById("features-feat");

window.addEventListener("scroll", () => {
  let scrollValue = window.scrollY;
  mainTitle.style.transform = `translateY(-${scrollValue * 0.2 - 200}px)`;
  mainTitle.style.opacity = 3.6 - scrollValue / 500;

  feat.style.transform = `translateX(-${scrollValue * 0.1 - 110}px)`;
  feat.style.opacity = 4.7 - scrollValue / 500;

});


const wMainTitle = document.getElementById("work-main_title");
let cards = document.getElementById("cards-container");
let button = document.getElementById("buttons-container");
window.addEventListener("scroll", () => {
  let scrollValue = window.scrollY;
  wMainTitle.style.transform = `translateY(-${scrollValue * 0.1 - 180}px)`;
  wMainTitle.style.opacity = 5.5 - scrollValue / 500;

  cards.style.transform = `translateX(-${scrollValue * 0.1 - 265}px)`;
  cards.style.opacity = 7 - scrollValue / 500;

  button.style.transform = `translateY(${scrollValue * 0.1 - 250}px)`;
  button.style.opacity = 7 - scrollValue / 500;

});
