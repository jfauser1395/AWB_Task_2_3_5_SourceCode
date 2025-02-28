// ######################################################## Main Program ########################################################

// Load the stock prices from the CSV file into a linked list
var stockPriceLinkedList = Preprocessor.LoadPricesLinkedList(0, 2);

// Get the head node of the linked list
Node<(DateTime, float)>? firstStockPrice = stockPriceLinkedList.Head;

// Iterative approach to find the node with the highest stock price
Node<(DateTime, float)> FindStockHighIteration(Node<(DateTime, float)>? currentNode)
{
    if (currentNode == null)
    {
        throw new ArgumentNullException(nameof(currentNode), "The linked list is empty.");
    }

    // Initialize maxNode with the first node
    Node<(DateTime, float)> maxNode = currentNode;
    currentNode = currentNode.Next; // Move to the next node

    // Traverse the linked list
    while (currentNode != null)
    {
        // Compare the stock price (second item in the tuple)
        if (currentNode.Value.Item2 > maxNode.Value.Item2)
        {
            maxNode = currentNode;
        }
        currentNode = currentNode.Next; // Move to the next node
    }

    return maxNode;
}

// Recursive approach to find the node with the highest stock price
Node<(DateTime, float)> FindStockHighRecursion(
    Node<(DateTime, float)>? currentNode,
    Node<(DateTime, float)> maxNode
)
{
    // Base case: if the current node is null, return the max node
    if (currentNode == null)
    {
        return maxNode;
    }

    // Compare the stock price (second item in the tuple)
    if (currentNode.Value.Item2 > maxNode.Value.Item2)
    {
        maxNode = currentNode;
    }

    // Recursive call with the next node and updated max node
    return FindStockHighRecursion(currentNode.Next, maxNode);
}

if (firstStockPrice != null)
{
    // Preserve the original head
    var originalHead = firstStockPrice;

    // Use a temporary pointer for printing the stock prices
    var tempPointer = firstStockPrice;
    int week = 1;
    while (tempPointer != null)
    {
        Console.WriteLine(
            $"Stock price on {tempPointer.Value.Item1:yyyy-MM-dd} Week {week}: ${tempPointer.Value.Item2:F2}"
        );
        tempPointer = tempPointer.Next;
        week++;
    }

    // Now use the preserved original head for finding the highest stock price
    var x = FindStockHighIteration(originalHead);
    Console.WriteLine(
        $"The highest price of the stock within 52 weeks: ${x.Value.Item2:F2} on {x.Value.Item1:yyyy-MM-dd}"
    );

    var y = FindStockHighRecursion(originalHead.Next, originalHead);
    Console.WriteLine(
        $"The highest price of the stock within 52 weeks: ${y.Value.Item2:F2} on {y.Value.Item1:yyyy-MM-dd}"
    );
}
else
{
    Console.WriteLine("The linked list is empty.");
}

// ######################################################## Linked List ##############################################################
public class Node<T>(T value) // Primary constructor to create a new node with specified value
{
    public T Value { get; set; } = value; // Property to hold the node's value
    public Node<T>? Next { get; set; } = null; // Property to hold the next node in the list
}

public class LinkedList<T>
{
    public Node<T>? Head { get; set; } // The head node of the linked list
    private int _count; // Private field

    public LinkedList() // Constructor to initialize the linked list
    {
        Head = null; // Initialize the head node to null
        _count = 0; // Initialize the count to zero
    }

    public int Count => _count; // Expression-bodied property to return the count

    // Add a new node to the linked list
    public void AddNewNode(T value)
    {
        var newNode = new Node<T>(value)
        {
            Next = Head // Set the next node to the current head
        };
        Head = newNode; // Update the head to the new node
        _count++; // Increment the count of nodes
    }

    // Add a new node after a given node
    public void AddAfter(Node<T> targetNode, T value)
    {
        if (targetNode == null)
        {
            throw new ArgumentNullException(nameof(targetNode), "Target node cannot be null.");
        }

        // Create the new node
        var newNode = new Node<T>(value)
        {
            Next = targetNode.Next // The new node points to the next node of the target
        };

        targetNode.Next = newNode; // The target node points to the new node
        _count++; // Increment count
    }
}

//####################################################### File processing block ####################################################
class Preprocessor
{
    // Get the first CSV file in the directory
    public static string GetCsvFile()
    {
        try
        {
            // Ensure the Files directory exists and throw an exception if it's missing
            string DirPath = Path.Combine(Directory.GetCurrentDirectory(), "StockPriceCSV");
            if (!Directory.Exists(DirPath))
            {
                throw new DirectoryNotFoundException($"The directory: '{DirPath}' does not exist.");
            }

            // Save all files into the xmlFilePaths string array and throw an exception if the folder is empty
            string[] filePaths = Directory.GetFiles(DirPath);
            if (filePaths.Length == 0)
            {
                throw new FileNotFoundException(
                    "No files found in the XMLFiles directory. Include a valid XML file in the XMLFiles folder."
                );
            }

            // Declare file name and extension then check the extension validity
            string firstFileExtension = Path.GetExtension(filePaths[0]).ToLower();
            string firstFileName = Path.GetFileName(filePaths[0]);
            if (string.IsNullOrEmpty(firstFileExtension))
            {
                throw new InvalidDataException(
                    $"File: '{firstFileName}' has no file extension. Please provide a valid CSV file."
                );
            }
            else if (firstFileExtension != ".csv") // Check if the file extension is not a CSV file
            {
                throw new InvalidDataException(
                    $"File: '{firstFileName}' has an incorrect file extension: '{firstFileExtension}'. Please provide a valid CSV file."
                );
            }

            return filePaths[0]; // Return the first file path if all checks pass
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"Directory error: {ex.Message}");
            // Additional handling for directory issues if needed
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"File error: {ex.Message}");
            // Additional handling for file issues if needed
        }
        catch (InvalidDataException ex)
        {
            Console.WriteLine($"Data error: {ex.Message}");
            // Additional handling for data issues if needed
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            // Handle unexpected exceptions
        }

        return string.Empty; // Return a default value in case of an error
    }

    // ######################################################## Sort and load CVS data into linked list #####################################################

    // Load the price data from the CSV file into a linked list
    public static LinkedList<(DateTime, float)> LoadPricesLinkedList(
        int dateColumnIndex,
        int priceColumnIndex,
        string? csvPath = null
    )
    {
        try
        {
            // Check if the CSV path is null or empty; if so, retrieve the CSV file path
            if (string.IsNullOrEmpty(csvPath))
            {
                csvPath = GetCsvFile(); // Get the first CSV file in the directory
            }

            // Create a linked list to store the prices
            var priceList = new LinkedList<(DateTime, float)>();

            // Read all lines from the CSV file and skip the header line
            var csvFileLines = File.ReadAllLines(csvPath).Skip(1).ToArray();

            // Check if there are at least 52 weeks of data; throw an exception if not
            if (csvFileLines.Length < 52)
            {
                throw new InvalidOperationException(
                    "CSV file does not contain at least 52 weeks of data."
                );
            }

            // Split the line using both ';' and ',' as delimiters
            string[] firstValues = csvFileLines[0].Split(';', ',');

            // Extract the raw date and price strings.
            string firstRawDate = firstValues[dateColumnIndex];
            string firstRawPrice = firstValues[priceColumnIndex].Trim('$', ' ', '"');

            // Try parsing the date and price to enter first node in the linked list
            if (
                DateTime.TryParse(firstRawDate, out DateTime firstDate)
                && float.TryParse(firstRawPrice, out float firstPrice)
            )
            {
                priceList.AddNewNode((firstDate, firstPrice));
            }
            else
            {
                throw new InvalidOperationException(
                    "Error parsing the first line of the CSV file."
                );
            }

            int week = 1;

            // Process exactly 52 lines
            for (int i = 1; i < 52; i++)
            {
                // increment the week
                week++;

                // Split the line using both ';' and ',' as delimiters
                string[] values = csvFileLines[i].Split(';', ',');

                // Extract the raw date and price strings.
                string rawDate = values[dateColumnIndex];
                string rawPrice = values[priceColumnIndex].Trim('$', ' ', '"');

                // Try parsing the date and price and check for null value in the head node
                if (
                    DateTime.TryParse(rawDate, out DateTime date)
                    && float.TryParse(rawPrice, out float price)
                    && priceList.Head != null
                )
                {
                    // Insert the parsed tuple into the linked list in order
                    // If the linked list is empty or the new entry's date is earlier than the first node's date, add it at the beginning
                    if (priceList.Count == 1 || date < priceList.Head.Value.Item1)
                    {
                        priceList.AddNewNode((date, price));
                    }
                    else
                    {
                        // Traverse the linked list to find the correct insertion point
                        Node<(DateTime date, float price)> current = priceList.Head;
                        while (current.Next != null && current.Next.Value.date < date)
                        {
                            current = current.Next;
                        }

                        // Insert the new entry after the current node
                        priceList.AddAfter(current, (date, price));
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Error parsing line {i + 2} of the CSV file."
                    );
                }
            }

            return priceList; // Return the linked list containing the price data
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"An error occurred while loading the prices: {ex.Message}");
            return new LinkedList<(DateTime, float)>(); // Return an empty linked list in case of an error
        }
    }
}
