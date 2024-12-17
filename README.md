# 3deye-test
Test task for 3dEYE company

## **Large File Generator and Sorter**

The task explanation can be found in the root of the repo - [Test Task Sorting v.01.pdf](https://github.com/VlaSiv/3deye-test/blob/master/Test%20Task%20Sorting%20v.01.pdf)

This solution includes two programs:

1. **File Generator**: Generates a large text file with each line formatted as `Number. String`. It generates lines in batches to optimize memory usage.
2. **File Sorter**: Sorts the large file in an efficient manner using batch-based sorting and merging to handle extremely large files (e.g., ~100GB).

---

### **Overview of Components**

#### **FileGenerator**
- **Purpose**: Creates a text file with a specified size in MB.
- **Approach**:
    - Generates random lines in the format `Number. String`.
    - Writes data in *batches* to avoid memory overflow.

#### **FileSorter**
- **Purpose**: Sorts a large file using a batch-based approach with external merge sort.
- **Approach**:
    1. Splits the input file into smaller batches.
    2. Sorts each batch and saves it as a temporary file.
    3. Merges all the sorted batches into a final output file.

---

### **Setup Instructions**

#### **Prerequisites**
- .NET 8.0 SDK installed.
- Visual Studio or any compatible C# IDE.

#### **Build**
Run the following command to build the project:
```bash
dotnet build
```

#### **Run**
1. **Generate a Test File**
   Use the `FileGenerator` to create a test file:
   ```bash
   dotnet run -- <outputFilePath> <fileSizeInMB>
   ```
   Example:
   ```bash
   dotnet run -- large_test_file.txt 100
   ```

2. **Sort the File**
   Use the `FileSorter` to sort the file:
   ```bash
   dotnet run -- <inputFilePath> <outputFilePath> <tempDirectory>
   ```
   Example:
   ```bash
   dotnet run -- large_test_file.txt sorted_output.txt temp_batches
   ```

---

### **Approach Details**

1. **Dynamic Batch Sizing**:
   - Adjusts batch size based on available system memory (~10%).
   - Ensures efficient memory usage and scalability.

2. **Parallel Processing**:
   - Line generation in `FileGenerator` uses multi-threading.
   - Batch sorting in `FileSorter` uses **PLINQ** for multi-core processing.

3. **Efficient Merge Phase**:
   - Uses **`PriorityQueue`** for optimal performance during batch merging.
   - Reduces overhead and improves scalability.

4. **Thread-Safe File Size Generation**:
   - Ensures precise file size generation by truncating excess lines in the last batch.

5. **Time Tracking**:
   - Execution time for file generation and sorting is logged in `hh:mm:ss` format.

---

### **Performance Considerations**
- **Scalability**: Designed to handle files up to hundreds of GB.
- **Memory Efficiency**: Optimizes batch size dynamically to fit system memory constraints.
- **CPU Utilization**: Leverages parallelism for line generation and batch sorting.
- **I/O Optimization**: Buffered reads/writes minimize disk I/O overhead.

---

### **Example**
**Input:**
```
415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow
```

**Output:**
```
1. Apple
415. Apple
2. Banana is yellow
32. Cherry is the best
30432. Something something something
```

## Usage Notes
- Ensure sufficient disk space for both temporary files and final output.
- For very large files, use a fast SSD to minimize I/O bottlenecks.
- Clean up temporary directories (`temp_batches`) after sorting to free disk space.