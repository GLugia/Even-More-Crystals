using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace Loot.Core.Recipes
{
	public static class RecipeHandler
	{
		internal static List<(object, int, object[], int[], int)> recipeList;

		internal static void Initialize()
		{
			recipeList = new List<(object, int, object[], int[], int)>();
		}

		/// <summary>
		/// Call this in SetDefaults to add a recipe to the list which will be iterated by this mod and added accordingly
		/// </summary>
		public static void AddRecipe(object result, int resultAmount, object[] requiredItems, int[] respectiveAmounts, int tileIDRequiredForCrafting = -1)
		{
			recipeList.Add((result, resultAmount, requiredItems, respectiveAmounts, tileIDRequiredForCrafting));
		}

		internal static void IterateRecipes()
		{
			foreach (var recipe in recipeList)
			{
				ModRecipe newRecipe = new ModRecipe(Loot.Instance);

				if (recipe.Item1.GetType().IsSubclassOf(typeof(Item)))
				{
					Item theItem = recipe.Item1 as Item;
					Loot.Logger.Debug("Adding recipe for " + theItem.GetType().Name);
					newRecipe.SetResult(theItem.type, recipe.Item2);
				}
				else if (recipe.Item1.GetType().IsSubclassOf(typeof(ModItem)))
				{
					ModItem theItem = recipe.Item1 as ModItem;
					Loot.Logger.Debug("Adding recipe for " + theItem.GetType().Name);
					newRecipe.SetResult(theItem.item.type, recipe.Item2);
				}
				else if (recipe.Item1.GetType().IsSubclassOf(typeof(Tile)))
				{
					Tile theTile = recipe.Item1 as Tile;
					Loot.Logger.Debug("Adding recipe for " + theTile.GetType().Name);
					newRecipe.SetResult(theTile.type, recipe.Item2);
				}
				else if (recipe.Item1.GetType().IsSubclassOf(typeof(ModTile)))
				{
					ModTile theTile = recipe.Item1 as ModTile;
					Loot.Logger.Debug("Adding recipe for " + theTile.GetType().Name);
					newRecipe.SetResult(theTile.Type, recipe.Item2);
				}
				else
				{
					throw new InvalidCastException("Recipe result object can only be of Item or Tile");
				}

				for (int i = 0; i < recipe.Item3.Length; i++)
				{
					if (recipe.Item3[i].GetType().IsSubclassOf(typeof(Item)))
					{
						Item theItem = recipe.Item3[i] as Item;
						Loot.Logger.Debug("Adding ingredient for " + theItem.GetType().Name);
						newRecipe.AddIngredient(theItem.type, recipe.Item4[i]);
					}
					else if (recipe.Item3[i].GetType().IsSubclassOf(typeof(ModItem)))
					{
						ModItem theItem = recipe.Item3[i] as ModItem;
						Loot.Logger.Debug("Adding ingredient for " + theItem.GetType().Name);
						newRecipe.AddIngredient(theItem.item.type, recipe.Item4[i]);
					}
					else if (recipe.Item3[i].GetType().IsSubclassOf(typeof(Tile)))
					{
						Tile theTile = recipe.Item3[i] as Tile;
						Loot.Logger.Debug("Adding ingredient for " + theTile.GetType().Name);
						newRecipe.AddIngredient(theTile.type, recipe.Item4[i]);
					}
					else if (recipe.Item3[i].GetType().IsSubclassOf(typeof(ModTile)))
					{
						ModTile theTile = recipe.Item3[i] as ModTile;
						Loot.Logger.Debug("Adding ingredient for " + theTile.GetType().Name);
						newRecipe.AddIngredient(theTile.Type, recipe.Item4[i]);
					}
					else
					{
						throw new InvalidCastException("Recipe ingredient objects can only be of Item or Tile");
					}
				}

				if (recipe.Item5 != -1)
				{
					Loot.Logger.Debug("Adding required crafting bench: " + ModContent.GetModTile(recipe.Item5));
					newRecipe.AddTile(recipe.Item5);
				}

				newRecipe.AddRecipe();
				Loot.Logger.Debug("Recipe added");
			}

			recipeList.Clear();
			recipeList.TrimExcess();
			recipeList = null;
		}
	}
}
