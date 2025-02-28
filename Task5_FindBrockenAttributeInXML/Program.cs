//####################################################### Main program ##############################################################
using System.Text.RegularExpressions;

var errorPatterns = new[]
{
    // Case0: Valid attribute with quoted value (basic valid case)
    @"([A-Za-z_:][A-Za-z0-9_.:-]*)\s*=\s*(?:""([^""\s]*)""|'([^'\s]*)')",
    // Case1: Empty attribute value: attribute = ""
    @"(\w+)\s*=\s*""\s*""",
    // Case2: Unterminated quotes: attribute="value
    @"\b(\w+)\s*=\s*([""'])[^""']*($|(?=\s|>|/))",
    // Case3: value without quotes: attribute=value
    @"\b(\w+)\s*=\s*(?![""'])([^\s""'=<>`]+)(?=[\s/>]|$)",
    // Case4: Missing starting quote: attribute=value"
    @"\b(\w+)\s*=\s*(?![""'])([^\s""']+)""",
    // Case5: Missing equals sign: attribute"value"
    @"\b(\w+)([""'])([^\s""']*)\2",
};

// Pattern to match invalid characters in attribute values
char[] invalidChars = ['<', '>', '&', '\'', '"'];

try
{
    // XML file processing block
    string[] xmlFilePaths = GetXmlFilesFromDirectory("XMLFiles");

    // Process the first XML file
    ProcessXmlFile(xmlFilePaths[0], 0);
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

//####################################################### Helper methods ##############################################################

// Get all XML files from a directory
string[] GetXmlFilesFromDirectory(string dirName)
{
    string dirPath = Path.Combine(Directory.GetCurrentDirectory(), dirName);

    // Check if directory exists
    if (!Directory.Exists(dirPath))
    {
        throw new DirectoryNotFoundException($"The directory: '{dirPath}' does not exist.");
    }

    // Get all files from the directory
    string[] files = Directory.GetFiles(dirPath);
    if (files.Length == 0)
    {
        throw new FileNotFoundException("No files found in the XMLFiles directory.");
    }

    // Filter the XML files and check that there's at least one .xml file
    string[] xmlFilePaths = files
        .Where(file => Path.GetExtension(file).ToLower() == ".xml")
        .ToArray();
    if (xmlFilePaths.Length == 0)
    {
        throw new InvalidDataException("No valid XML files found in the directory.");
    }

    // Validate the first XML file's extension (if needed)
    string firstFileExtension = Path.GetExtension(xmlFilePaths[0]).ToLower();
    if (string.IsNullOrEmpty(firstFileExtension) || firstFileExtension != ".xml")
    {
        throw new InvalidDataException(
            $"File: '{Path.GetFileName(xmlFilePaths[0])}' has an invalid extension."
        );
    }

    return xmlFilePaths; // Return the valid XML file paths
}

void ProcessXmlFile(string filePath, int lineIndex)
{
    // Read all lines from the XML file
    string[] XMLFileLines = File.ReadAllLines(filePath);

    // Base case: If all lines have been processed, return
    if (lineIndex >= XMLFileLines.Length)  return;

    // Get the current line and check for errors
    string line = XMLFileLines[lineIndex];

    ProcessXmlAttributesRecursively(line, lineIndex);

    // Recur for the next line
    ProcessXmlFile(filePath, lineIndex + 1);
}

void ProcessXmlAttributesRecursively(string input, int lineIndex, int patternIndex = 0)
{
    // Base case: if all patterns have been processed, stop recursion
    if (patternIndex >= errorPatterns.Length) return;

    // Create a HashSet to track seen attribute names
    var seenAttributeNames = new HashSet<string>();

    // Get all matches for the current pattern
    var matches = Regex.Matches(input, errorPatterns[patternIndex]);

    // If matches are found, process them
    if (matches.Count > 0)
    {
        // Call the new recursive method to process matches
        ProcessMatchesRecursively(
            matches,
            0,
            lineIndex,
            seenAttributeNames,
            invalidChars,
            patternIndex
        );
    }

    // Recur with the next pattern
    ProcessXmlAttributesRecursively(input, lineIndex, patternIndex + 1);
}

// Process matches recursively based on the pattern index
void ProcessMatchesRecursively(
    MatchCollection matches,
    int currentIndex,
    int lineIndex,
    HashSet<string> seenAttributeNames,
    char[] invalidChars,
    int patternIndex
)
{
    if (currentIndex >= matches.Count) return;

    Match match = matches[currentIndex];
    string matchString = match.Value;

    // Only validate attributes for properly formatted matches (patternIndex 0)
    if (patternIndex == 0)
    {
        // Split the match into attribute name and value parts
        var parts = matchString.Split(new[] { '=', '"' }, 2);
        if (parts.Length == 2)
        {
            string attributeName = parts[0].Trim();
            string attributeValue = parts[1].Trim('"');

            // Duplicate check
            if (seenAttributeNames.Contains(attributeName))
            {
                Console.WriteLine(
                    $"On line {lineIndex + 1} Duplicate found Attribute: {attributeName}"
                );
            }
            else // Add the attribute name to the set
            {
                seenAttributeNames.Add(attributeName);
            }

            // Validate attribute value
            if (attributeValue.IndexOfAny(invalidChars) >= 0)
            {
                Console.WriteLine(
                    $"On line {lineIndex + 1} Invalid character found in value: {attributeValue}"
                );
            }
        }
        else
        {
            throw new InvalidDataException("Unexpected attribute format: " + matchString);
            // Handle unexpected attribute format
        }
    }
    else // Handle special error cases
    {
        // Pattern-specific handling
        switch (patternIndex)
        {
            case 1:
                Console.WriteLine($"On line {lineIndex + 1} Empty attribute value: {match.Value}");
                break;
            case 2:
                Console.WriteLine($"On line {lineIndex + 1} Unterminated quotes: {match.Value}");
                break;
            case 3:
                Console.WriteLine(
                    $"On line {lineIndex + 1} Attribute: {match.Value} missing both quotes"
                );
                break;
            case 4:
                Console.WriteLine(
                    $"On line {lineIndex + 1} Attribute: {match.Value} missing starting quote"
                );
                break;
            case 5:
                Console.WriteLine(
                    $"On line {lineIndex + 1} Attribute {match.Value} missing equals sign"
                );
                break;
        }
    }

    // Process next match recursively
    ProcessMatchesRecursively(
        matches,
        currentIndex + 1,
        lineIndex,
        seenAttributeNames,
        invalidChars,
        patternIndex
    );
}
