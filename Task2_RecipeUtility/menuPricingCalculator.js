//######################################## Global variables ########################################

// Define conversion factors for weight & volume
const unitConversions = {
  g: 0.001, // 1 gram = 0.001 kg
  kg: 1, // 1 kilogram = 1 kg
  ml: 0.001, // 1 milliliter = 0.001 L
  L: 1, // 1 liter = 1 L
  piece: 1, // Per piece stays the same
};

// Tax rate is 10%
const TAX_RATE = 0.1;

// Ingredient cost must be 80% of the card price, 10% tax on top of the selling price
const INGREDIENT_COST_PERCENTAGE = 0.8;
const SELLING_PRICE_FACTOR = INGREDIENT_COST_PERCENTAGE * (1 - TAX_RATE); // 0.72 in this case

// dish object
let dish = {
  dishName: "", // Name of the dish
  recipe: [], // List of ingredients in the recipe
  taxes: 0, // Taxes on the dish
  totalIngredientCost: 0, // Total cost of the ingredients in the dish
  profitMargin: 0, // Profit margin of the dish
  sellingPrice: 0, // Selling price of the dish
};

// menu card array
let menuCard = [];

// ################################################## Functions ##################################################

// Add a new ingredient to the recipe
function addIngredientToRecipe(
  ingredientName,
  pricePerUnit,
  pricingUnit,
  amountNeeded,
  amountUnit
) {

  // Determine the conversion factor
  // If pricing is per "piece", no conversion is needed (factor = 1).
  // Otherwise, use the conversion factor for the measurement unit.
  const conversionFactor =
    pricingUnit === "piece" ? 1 : unitConversions[amountUnit];

  // Create an ingredient object.
  const ingredient = {
    ingredientName, // Name of the ingredient
    pricePerUnit, // Price per unit in dollars (e.g., $/kg) purchasing price
    pricingUnit, // Pricing unit of the ingredient (e.g., kg, L, piece)
    amountNeeded, // Amount of the ingredient needed in the recipe
    amountUnit, // Measurement unit of the ingredient (e.g., g, ml, piece)
    totalCost: amountNeeded * pricePerUnit * conversionFactor, // Total cost of the ingredient in dollars
  };

  // Add the ingredient to the recipe's ingredient list.
  dish.recipe.push(ingredient);
}

// Add a new dish to the menu card
function addDishToCard(dishName) {
  
  // Set the dish name
  dish.dishName = dishName;

  // calculate total cost of dish
  dish.recipe.forEach((ingredient) => {
    dish.totalIngredientCost += ingredient.totalCost;
  });

  // calculate selling price
  dish.sellingPrice = dish.totalIngredientCost / SELLING_PRICE_FACTOR;

  //calculate taxes
  dish.taxes = dish.sellingPrice * TAX_RATE;

  // calculate profit margin
  dish.profitMargin = dish.sellingPrice - dish.totalIngredientCost - dish.taxes;

  // Add the dish to the dishes array
  menuCard.push(dish);

  // Reset the recipe object
  dish = {
    dishName: "",
    recipe: [],
    taxes: 0,
    totalIngredientCost: 0,
    profitMargin: 0,
    sellingPrice: 0,
  };
}

//#################################### Dom Event Handling ######################################

// Handle the click event on the "Add Ingredient" button
function enterIngredients() {
  // Get the input values
  const ingredientName = document.getElementById("ingredientName");
  const pricePerUnit = document.getElementById("pricePerUnit");
  const pricingUnit = document.getElementById("pricingUnit");
  const amountNeeded = document.getElementById("amountNeeded");
  const amountUnit = document.getElementById("amountUnit");
  const ingredientList = document.getElementById("ingredientList");

  // Add ingredient to recipe
  addIngredientToRecipe(
    ingredientName.value,
    parseFloat(pricePerUnit.value),
    pricingUnit.value,
    parseFloat(amountNeeded.value),
    amountUnit.value
  );

  // Clear input and output values after adding
  ingredientName.value = "";
  pricePerUnit.value = "";
  pricingUnit.value = "";
  amountNeeded.value = "";
  amountUnit.value = "";
  ingredientList.innerHTML = "";

  // Show the recipe list

  dish.recipe.forEach((ingredient) => {
    const newIngredient = document.createElement("li");
    newIngredient.innerHTML = `${ingredient.ingredientName}:
    ${ingredient.amountNeeded} ${ingredient.amountUnit}
    for ${ingredient.totalCost.toFixed(2)}$ `;
    ingredientList.appendChild(newIngredient);
  });
}

// Handle the click event on the "Add Dish" button
function enterDishName() {
  // Get the input value
  const dishList = document.getElementById("dishList");
  const ingredientList = document.getElementById("ingredientList");
  const dishNameInput = document.getElementById("newRecipe");

  // Add dish to menu card
  addDishToCard(dishNameInput.value);

  // Clear dish name input after adding
  dishList.innerHTML = "";
  dishNameInput.value = "";
  ingredientList.innerHTML = "";

  // Show dish name
  menuCard.forEach((dish) => {
    const newDish = document.createElement("li");
    newDish.innerHTML = `${dish.dishName}`;
    dishList.appendChild(newDish);
  });
}

// Handle the click event on the "Generate Menu" button
function generateTable() {
  // Show the menu card
  const dishList = document.getElementById("dishList");
  const calcTableBody = document.getElementById("calcTableBody");
  const menuTableBody = document.getElementById("menuTableBody");

  // Clear previous table rows
  dishList.innerHTML = "";
  calcTableBody.innerHTML = "";
  menuTableBody.innerHTML = "";

  // Generate menu card
  menuCard.forEach((dish) => {
    // Generate calculation table
    const calcTableRow = document.createElement("tr");
    calcTableRow.innerHTML = `
    <td>${dish.dishName}</td>
    <td>${dish.sellingPrice.toFixed(2)}$</td>
    <td>${dish.taxes.toFixed(2)}$</td>
    <td>${dish.totalIngredientCost.toFixed(2)}$</td>
    <td>${dish.profitMargin.toFixed(2)}$</td>
     <td> 
      <ul>
        ${dish.recipe
          .map(
            (ingredient) =>
              `<li>${ingredient.ingredientName}: ${ingredient.totalCost.toFixed(2)}$</li>`
          )
          .join("")}
      </ul>
    </td>
    `;
    calcTableBody.appendChild(calcTableRow); // Append the row to the table

    // Generate menu table
    const menuTableRow = document.createElement("tr");
    menuTableRow.innerHTML = `
    <td>${dish.dishName}</td> 
    <td>${dish.sellingPrice.toFixed(2)}$</td>
    `;
    menuTableBody.appendChild(menuTableRow); // Append the row to the table
  });

  // Show the table
  const tableContainer = document.getElementById("tableContainer");
  tableContainer.hidden = tableContainer.hidden ? false : true;
}
