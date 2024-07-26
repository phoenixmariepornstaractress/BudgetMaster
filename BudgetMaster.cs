using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace BudgetTracker
{
    class Program
    {
        static void Main(string[] args)
        {
            BudgetTracker tracker = new BudgetTracker();
            tracker.LoadData();
            bool running = true;

            while (running)
            {
                Console.Clear();
                Console.WriteLine("Budget Tracker");
                Console.WriteLine("1. Add Income");
                Console.WriteLine("2. Add Expense");
                Console.WriteLine("3. View Summary");
                Console.WriteLine("4. View Transactions");
                Console.WriteLine("5. Set Savings Goal");
                Console.WriteLine("6. Edit Transaction");
                Console.WriteLine("7. Delete Transaction");
                Console.WriteLine("8. Generate Reports");
                Console.WriteLine("9. Search Transactions");
                Console.WriteLine("10. Export Transactions to CSV");
                Console.WriteLine("11. Manage Categories");
                Console.WriteLine("12. Manage Recurring Transactions");
                Console.WriteLine("13. Manage User Profiles");
                Console.WriteLine("14. Save and Exit");
                Console.Write("Choose an option: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        tracker.AddIncome();
                        break;
                    case "2":
                        tracker.AddExpense();
                        break;
                    case "3":
                        tracker.ViewSummary();
                        break;
                    case "4":
                        tracker.ViewTransactions();
                        break;
                    case "5":
                        tracker.SetSavingsGoal();
                        break;
                    case "6":
                        tracker.EditTransaction();
                        break;
                    case "7":
                        tracker.DeleteTransaction();
                        break;
                    case "8":
                        tracker.GenerateReports();
                        break;
                    case "9":
                        tracker.SearchTransactions();
                        break;
                    case "10":
                        tracker.ExportTransactionsToCsv();
                        break;
                    case "11":
                        tracker.ManageCategories();
                        break;
                    case "12":
                        tracker.ManageRecurringTransactions();
                        break;
                    case "13":
                        tracker.ManageUserProfiles();
                        break;
                    case "14":
                        tracker.SaveData();
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }
    }

    public class BudgetTracker
    {
        private List<Transaction> transactions = new List<Transaction>();
        private decimal savingsGoal = 0;
        private List<Category> categories = new List<Category>();
        private List<RecurringTransaction> recurringTransactions = new List<RecurringTransaction>();
        private UserProfile currentUser = new UserProfile();
        private List<UserProfile> userProfiles = new List<UserProfile>();
        private const string DataFile = "budget_data.json";
        private const decimal OverspendingThreshold = 0.8m;

        public void AddIncome()
        {
            Console.Write("Enter income description: ");
            string description = Console.ReadLine();
            Console.Write("Enter amount: ");
            decimal amount = decimal.Parse(Console.ReadLine());
            Console.Write("Enter category: ");
            string category = Console.ReadLine();
            transactions.Add(new Transaction(description, amount, category, TransactionType.Income));
            CheckOverspending();
        }

        public void AddExpense()
        {
            Console.Write("Enter expense description: ");
            string description = Console.ReadLine();
            Console.Write("Enter amount: ");
            decimal amount = decimal.Parse(Console.ReadLine());
            Console.Write("Enter category: ");
            string category = Console.ReadLine();
            transactions.Add(new Transaction(description, -amount, category, TransactionType.Expense));
            CheckOverspending();
        }

        public void ViewSummary()
        {
            Console.Clear();
            Console.WriteLine("Summary:");
            decimal totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            decimal totalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            decimal balance = totalIncome + totalExpenses;

            Console.WriteLine($"Total Income: {totalIncome:C}");
            Console.WriteLine($"Total Expenses: {totalExpenses:C}");
            Console.WriteLine($"Balance: {balance:C}");
            Console.WriteLine($"Savings Goal: {savingsGoal:C}");

            if (savingsGoal > 0)
            {
                decimal amountNeeded = savingsGoal - balance;
                if (amountNeeded > 0)
                {
                    Console.WriteLine($"You need {amountNeeded:C} more to reach your savings goal.");
                }
                else
                {
                    Console.WriteLine("Congratulations! You've reached your savings goal.");
                }
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        public void ViewTransactions()
        {
            Console.Clear();
            Console.WriteLine("Transactions:");
            for (int i = 0; i < transactions.Count; i++)
            {
                var transaction = transactions[i];
                Console.WriteLine($"{i + 1}. {transaction.Description} - {transaction.Amount:C} ({transaction.Type}) [Category: {transaction.Category}]");
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        public void SetSavingsGoal()
        {
            Console.Write("Enter savings goal amount: ");
            savingsGoal = decimal.Parse(Console.ReadLine());
        }

        public void EditTransaction()
        {
            Console.Write("Enter transaction number to edit: ");
            int index = int.Parse(Console.ReadLine()) - 1;

            if (index >= 0 && index < transactions.Count)
            {
                Console.Write("Enter new description: ");
                transactions[index].Description = Console.ReadLine();
                Console.Write("Enter new amount: ");
                transactions[index].Amount = decimal.Parse(Console.ReadLine());
                Console.Write("Enter new category: ");
                transactions[index].Category = Console.ReadLine();
                Console.WriteLine("Transaction updated.");
            }
            else
            {
                Console.WriteLine("Invalid transaction number.");
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        public void DeleteTransaction()
        {
            Console.Write("Enter transaction number to delete: ");
            int index = int.Parse(Console.ReadLine()) - 1;

            if (index >= 0 && index < transactions.Count)
            {
                transactions.RemoveAt(index);
                Console.WriteLine("Transaction deleted.");
            }
            else
            {
                Console.WriteLine("Invalid transaction number.");
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        public void GenerateReports()
        {
            Console.Clear();
            Console.WriteLine("1. Monthly Report");
            Console.WriteLine("2. Yearly Report");
            Console.Write("Choose an option: ");
            var option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    GenerateMonthlyReport();
                    break;
                case "2":
                    GenerateYearlyReport();
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        private void GenerateMonthlyReport()
        {
            var groupedByMonth = transactions.GroupBy(t => new { t.Date.Year, t.Date.Month });
            foreach (var group in groupedByMonth)
            {
                decimal income = group.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                decimal expenses = group.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                Console.WriteLine($"{group.Key.Year}-{group.Key.Month}: Income: {income:C}, Expenses: {expenses:C}, Balance: {income + expenses:C}");
            }
        }

        private void GenerateYearlyReport()
        {
            var groupedByYear = transactions.GroupBy(t => t.Date.Year);
            foreach (var group in groupedByYear)
            {
                decimal income = group.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                decimal expenses = group.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                Console.WriteLine($"{group.Key}: Income: {income:C}, Expenses: {expenses:C}, Balance: {income + expenses:C}");
            }
        }

        public void SearchTransactions()
        {
            Console.Write("Enter description or category to search: ");
            string query = Console.ReadLine().ToLower();

            var results = transactions.Where(t => t.Description.ToLower().Contains(query) || t.Category.ToLower().Contains(query)).ToList();

            if (results.Count > 0)
            {
                Console.WriteLine("Search Results:");
                for (int i = 0; i < results.Count; i++)
                {
                    var transaction = results[i];
                    Console.WriteLine($"{i + 1}. {transaction.Description} - {transaction.Amount:C} ({transaction.Type}) [Category: {transaction.Category}]");
                }
            }
            else
            {
                Console.WriteLine("No matching transactions found.");
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        public void ExportTransactionsToCsv()
        {
            Console.Write("Enter the CSV file name (e.g., transactions.csv): ");
            string fileName = Console.ReadLine();

            using (var writer = new StreamWriter(fileName))
            {
                writer.WriteLine("Description,Amount,Category,Type,Date");
                foreach (var transaction in transactions)
                {
                    writer.WriteLine($"{transaction.Description},{transaction.Amount},{transaction.Category},{transaction.Type},{transaction.Date}");
                }
            }

            Console.WriteLine($"Transactions exported to {fileName}");
            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        public void ManageCategories()
        {
            Console.Clear();
            Console.WriteLine("Manage Categories:");
            Console.WriteLine("1. Add Category");
            Console.WriteLine("2. View Categories");
            Console.WriteLine("3. Edit Category");
            Console.WriteLine("4. Delete Category");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    AddCategory();
                    break;
                case "2":
                    ViewCategories();
                    break;
                case "3":
                    EditCategory();
                    break;
                case "4":
                    DeleteCategory();
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        private void AddCategory()
        {
            Console.Write("Enter category name: ");
            string name = Console.ReadLine();
            Console.Write("Enter spending limit (optional, press Enter to skip): ");
            string limitInput = Console.ReadLine();
            decimal limit = string.IsNullOrEmpty(limitInput) ? decimal.MaxValue : decimal.Parse(limitInput);
            categories.Add(new Category { Name = name, SpendingLimit = limit });
            Console.WriteLine("Category added.");
        }

        private void ViewCategories()
        {
            Console.Clear();
            Console.WriteLine("Categories:");
            for (int i = 0; i < categories.Count; i++)
            {
                var category = categories[i];
                Console.WriteLine($"{i + 1}. {category.Name} - Limit: {category.SpendingLimit:C}");
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        private void EditCategory()
        {
            Console.Write("Enter category number to edit: ");
            int index = int.Parse(Console.ReadLine()) - 1;

            if (index >= 0 && index < categories.Count)
            {
                Console.Write("Enter new category name: ");
                categories[index].Name = Console.ReadLine();
                Console.Write("Enter new spending limit (optional, press Enter to skip): ");
                string limitInput = Console.ReadLine();
                categories[index].SpendingLimit = string.IsNullOrEmpty(limitInput) ? decimal.MaxValue : decimal.Parse(limitInput);
                Console.WriteLine("Category updated.");
            }
            else
            {
                Console.WriteLine("Invalid category number.");
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        private void DeleteCategory()
        {
            Console.Write("Enter category number to delete: ");
            int index = int.Parse(Console.ReadLine()) - 1;

            if (index >= 0 && index < categories.Count)
            {
                categories.RemoveAt(index);
                Console.WriteLine("Category deleted.");
            }
            else
            {
                Console.WriteLine("Invalid category number.");
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        public void ManageRecurringTransactions()
        {
            Console.Clear();
            Console.WriteLine("Manage Recurring Transactions:");
            Console.WriteLine("1. Add Recurring Transaction");
            Console.WriteLine("2. View Recurring Transactions");
            Console.WriteLine("3. Delete Recurring Transaction");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    AddRecurringTransaction();
                    break;
                case "2":
                    ViewRecurringTransactions();
                    break;
                case "3":
                    DeleteRecurringTransaction();
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        private void AddRecurringTransaction()
        {
            Console.Write("Enter transaction description: ");
            string description = Console.ReadLine();
            Console.Write("Enter amount: ");
            decimal amount = decimal.Parse(Console.ReadLine());
            Console.Write("Enter category: ");
            string category = Console.ReadLine();
            Console.Write("Enter frequency (Daily, Weekly, Monthly): ");
            RecurrenceFrequency frequency = (RecurrenceFrequency)Enum.Parse(typeof(RecurrenceFrequency), Console.ReadLine(), true);

            recurringTransactions.Add(new RecurringTransaction
            {
                Description = description,
                Amount = amount,
                Category = category,
                Frequency = frequency
            });

            Console.WriteLine("Recurring transaction added.");
        }

        private void ViewRecurringTransactions()
        {
            Console.Clear();
            Console.WriteLine("Recurring Transactions:");
            for (int i = 0; i < recurringTransactions.Count; i++)
            {
                var transaction = recurringTransactions[i];
                Console.WriteLine($"{i + 1}. {transaction.Description} - {transaction.Amount:C} ({transaction.Frequency}) [Category: {transaction.Category}]");
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        private void DeleteRecurringTransaction()
        {
            Console.Write("Enter recurring transaction number to delete: ");
            int index = int.Parse(Console.ReadLine()) - 1;

            if (index >= 0 && index < recurringTransactions.Count)
            {
                recurringTransactions.RemoveAt(index);
                Console.WriteLine("Recurring transaction deleted.");
            }
            else
            {
                Console.WriteLine("Invalid recurring transaction number.");
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        public void ManageUserProfiles()
        {
            Console.Clear();
            Console.WriteLine("Manage User Profiles:");
            Console.WriteLine("1. Add User Profile");
            Console.WriteLine("2. View User Profiles");
            Console.WriteLine("3. Switch User Profile");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    AddUserProfile();
                    break;
                case "2":
                    ViewUserProfiles();
                    break;
                case "3":
                    SwitchUserProfile();
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        private void AddUserProfile()
        {
            Console.Write("Enter user name: ");
            string name = Console.ReadLine();
            userProfiles.Add(new UserProfile { UserName = name });
            Console.WriteLine("User profile added.");
        }

        private void ViewUserProfiles()
        {
            Console.Clear();
            Console.WriteLine("User Profiles:");
            for (int i = 0; i < userProfiles.Count; i++)
            {
                var profile = userProfiles[i];
                Console.WriteLine($"{i + 1}. {profile.UserName}");
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        private void SwitchUserProfile()
        {
            Console.Write("Enter user profile number to switch: ");
            int index = int.Parse(Console.ReadLine()) - 1;

            if (index >= 0 && index < userProfiles.Count)
            {
                currentUser = userProfiles[index];
                Console.WriteLine($"Switched to user profile: {currentUser.UserName}");
            }
            else
            {
                Console.WriteLine("Invalid user profile number.");
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
        }

        public void SaveData()
        {
            var data = new BudgetData
            {
                Transactions = transactions,
                SavingsGoal = savingsGoal,
                Categories = categories,
                RecurringTransactions = recurringTransactions,
                UserProfiles = userProfiles,
                CurrentUser = currentUser
            };

            File.WriteAllText(DataFile, JsonConvert.SerializeObject(data));
            Console.WriteLine("Data saved.");
        }

        public void LoadData()
        {
            if (File.Exists(DataFile))
            {
                var data = JsonConvert.DeserializeObject<BudgetData>(File.ReadAllText(DataFile));
                transactions = data.Transactions ?? new List<Transaction>();
                savingsGoal = data.SavingsGoal;
                categories = data.Categories ?? new List<Category>();
                recurringTransactions = data.RecurringTransactions ?? new List<RecurringTransaction>();
                userProfiles = data.UserProfiles ?? new List<UserProfile>();
                currentUser = data.CurrentUser ?? new UserProfile();
            }
        }

        private void CheckOverspending()
        {
            decimal totalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            decimal totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);

            if (totalExpenses > totalIncome * OverspendingThreshold)
            {
                Console.WriteLine("Warning: You are overspending!");
            }
        }
    }

    public class Transaction
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public TransactionType Type { get; set; }
        public DateTime Date { get; set; }

        public Transaction(string description, decimal amount, string category, TransactionType type)
        {
            Description = description;
            Amount = amount;
            Category = category;
            Type = type;
            Date = DateTime.Now;
        }
    }

    public enum TransactionType
    {
        Income,
        Expense
    }

    public class RecurringTransaction
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public RecurrenceFrequency Frequency { get; set; }
    }

    public enum RecurrenceFrequency
    {
        Daily,
        Weekly,
        Monthly
    }

    public class Category
    {
        public string Name { get; set; }
        public decimal SpendingLimit { get; set; }
    }

    public class UserProfile
    {
        public string UserName { get; set; }
    }

    public class BudgetData
    {
        public List<Transaction> Transactions { get; set; }
        public decimal SavingsGoal { get; set; }
        public List<Category> Categories { get; set; }
        public List<RecurringTransaction> RecurringTransactions { get; set; }
        public List<UserProfile> UserProfiles { get; set; }
        public UserProfile CurrentUser { get; set; }
    }
}
