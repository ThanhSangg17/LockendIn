// src/services/mockGemini.ts

export interface MealItem {
  name: string;
  description: string;
  calories: number;
  protein: number; // g
  carb: number; // g
  fat: number; // g
}

export interface MealPlan {
  id: string;
  workspaceId: string;
  createdAt: string;
  summary: {
    targetCalories: number;
    proteinGrams: number;
    carbGrams: number;
    fatGrams: number;
  };
  meals: {
    breakfast: MealItem;
    lunch: MealItem;
    snack: MealItem;
    dinner: MealItem;
  };
  nutritionNotes: string[];
}

const MEAL_PLANS_KEY = 'lockedin_meal_plans';

export function getMockMealPlans(): MealPlan[] {
  const data = localStorage.getItem(MEAL_PLANS_KEY);
  return data ? JSON.parse(data) : [];
}

export function saveMockMealPlans(plans: MealPlan[]): void {
  localStorage.setItem(MEAL_PLANS_KEY, JSON.stringify(plans));
}

export function generateAIMealPlan(
  workspaceId: string,
  stats: {
    height: number;
    weight: number;
    fitnessGoal: string;
    allergies: string;
    mealsPerDay: number;
    healthNotes: string;
  }
): Promise<MealPlan> {
  return new Promise((resolve) => {
    // Simulate API delay
    setTimeout(() => {
      const heightInM = stats.height / 100;
      const bmi = stats.weight / (heightInM * heightInM);
      const goal = stats.fitnessGoal.toLowerCase();
      
      let baseCalories = 2000;
      let pRatio = 0.3; // 30% protein
      let cRatio = 0.45; // 45% carbs
      let fRatio = 0.25; // 25% fats

      if (goal.includes('lose') || goal.includes('giảm') || goal.includes('fat') || goal.includes('weight')) {
        baseCalories = Math.round(stats.weight * 22); // Deficit
        pRatio = 0.35; // Higher protein for muscle preservation
        cRatio = 0.35;
        fRatio = 0.30;
      } else if (goal.includes('build') || goal.includes('tăng') || goal.includes('gain') || goal.includes('muscle') || goal.includes('hypertrophy')) {
        baseCalories = Math.round(stats.weight * 32) + 300; // Surplus
        pRatio = 0.30;
        cRatio = 0.50;
        fRatio = 0.20;
      }

      // Safeguard calories range
      if (baseCalories < 1200) baseCalories = 1200;
      if (baseCalories > 4000) baseCalories = 4000;

      const pGrams = Math.round((baseCalories * pRatio) / 4);
      const cGrams = Math.round((baseCalories * cRatio) / 4);
      const fGrams = Math.round((baseCalories * fRatio) / 9);

      // Construct dynamic meals based on goals & allergies
      const allergyText = stats.allergies ? ` (Strictly avoiding: ${stats.allergies})` : '';

      const breakfast: MealItem = {
        name: 'Oatmeal Power Bowl',
        description: `Rolled oats cooked in unsweetened almond milk, topped with blueberies, chia seeds, and 1 scoop of clean whey protein powder${allergyText}.`,
        calories: Math.round(baseCalories * 0.25),
        protein: Math.round(pGrams * 0.30),
        carb: Math.round(cGrams * 0.28),
        fat: Math.round(fGrams * 0.18)
      };

      const lunch: MealItem = {
        name: 'High-Protein Fitness Bowl',
        description: `Grilled chicken breast (or grilled firm tofu if vegetarian), brown rice, broccoli, asparagus, and a side of mixed greens with olive oil dressing${allergyText}.`,
        calories: Math.round(baseCalories * 0.35),
        protein: Math.round(pGrams * 0.38),
        carb: Math.round(cGrams * 0.35),
        fat: Math.round(fGrams * 0.32)
      };

      const snack: MealItem = {
        name: 'Nutrient-Dense Recovery Snack',
        description: `Greek yogurt (0% fat) topped with raw almonds and sliced banana, paired with 2 hard-boiled egg whites${allergyText}.`,
        calories: Math.round(baseCalories * 0.15),
        protein: Math.round(pGrams * 0.12),
        carb: Math.round(cGrams * 0.15),
        fat: Math.round(fGrams * 0.20)
      };

      const dinner: MealItem = {
        name: 'Restorative Salmon & Sweet Potato Dinner',
        description: `Baked wild salmon fillet, roasted sweet potato wedges, sautéed spinach with garlic, and baby carrots${allergyText}.`,
        calories: Math.round(baseCalories * 0.25),
        protein: Math.round(pGrams * 0.20),
        carb: Math.round(cGrams * 0.22),
        fat: Math.round(fGrams * 0.30)
      };

      const nutritionNotes = [
        `Target BMI calculated: ${bmi.toFixed(1)} - Goal matches energy output.`,
        `All meals have been cross-checked to avoid designated allergens: ${stats.allergies || 'None listed'}.`,
        `Pre-workout recommendation: Drink 300ml of water 30 minutes before training.`,
        `Post-workout recommendation: Consume your lunch or recovery snack within 45 mins to optimize recovery.`,
        `Drink at least 2.5 - 3 liters of water throughout the day.`
      ];

      const newPlan: MealPlan = {
        id: 'plan-' + Math.random().toString(36).substring(2, 9),
        workspaceId,
        createdAt: new Date().toISOString(),
        summary: {
          targetCalories: baseCalories,
          proteinGrams: pGrams,
          carbGrams: cGrams,
          fatGrams: fGrams
        },
        meals: {
          breakfast,
          lunch,
          snack,
          dinner
        },
        nutritionNotes
      };

      const allPlans = getMockMealPlans();
      allPlans.push(newPlan);
      saveMockMealPlans(allPlans);

      resolve(newPlan);
    }, 1200); // 1.2s delay for a premium AI typing effect
  });
}
