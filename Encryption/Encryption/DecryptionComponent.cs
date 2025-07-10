using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Grasshopper.Kernel;
using System.Threading.Tasks;


public class FileDecryptorComponent : GH_Component
{
    // Hardcoded Key and IV (for AES-128)
    private static readonly byte[] Key = Convert.FromBase64String("GZuHOfU1C5F6SgI+PRKo+g=="); // 16 bytes for AES  
    private static readonly byte[] IV = Convert.FromBase64String("0v2u51jxM1dwalKzbWiZHg=="); // 16 bytes for AES     


    public FileDecryptorComponent()
        : base("File Decryptor", "Decryptor", "Decrypt a file using AES encryption.", "Utilities", "Encryption")
    {
    }

    public override Guid ComponentGuid => new Guid("{5C080718-1F2B-442A-9FD5-EC752713A505}");

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddTextParameter("Input File Path", "I", "The path of the file to decrypt", GH_ParamAccess.list); // Changed to list  
        pManager.AddBooleanParameter("Delete After Decrypt", "D", "If true, the decrypted file and folder will be deleted after processing.", GH_ParamAccess.item, false);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Decrypted File Paths", "P", "Paths of the decrypted files.", GH_ParamAccess.list); // Updated to list  
        pManager.AddBooleanParameter("Success", "S", "True if the files were successfully decrypted.", GH_ParamAccess.item);
    }
    private List<string> decryptedFilePaths = new List<string>();
    string targetFolderPath = Path.Combine("C:\\PARDUS_CEPHE_YAZILIMI\\Pardus_CY\\01_BLOK", "08_PCY");

    protected override async void SolveInstance(IGH_DataAccess DA)
    {
        List<string> inputFilePaths = new List<string>();
        bool deleteAfterDecrypt = false;
        bool success = true;
        decryptedFilePaths.Clear(); // Clear previous entries to avoid duplicates  

        // Retrieve input data  
        if (!DA.GetDataList(0, inputFilePaths)) return; // Use GetDataList for multiple inputs  
        if (!DA.GetData(1, ref deleteAfterDecrypt)) return;


        // Check if the parent directory exists, and create the target folder if necessary  
        if (Directory.Exists(Path.Combine("C:\\PARDUS_CEPHE_YAZILIMI\\Pardus_CY\\01_BLOK")) == false)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Pardus directory does not exist.");
            return;
        }

        // Create the target folder if it does not exist  
        if (!Directory.Exists(targetFolderPath))
        {
            Directory.CreateDirectory(targetFolderPath);
        }

        if (!deleteAfterDecrypt)
        { 
            try
            {
                foreach (var inputFilePath in inputFilePaths)
            {
                string originalFileName = Path.GetFileNameWithoutExtension(inputFilePath);
                string decryptedFilePath = Path.Combine(targetFolderPath, originalFileName + ".3dm");


                // Decrypt the file  
                DecryptFile(inputFilePath, decryptedFilePath);
                decryptedFilePaths.Add(decryptedFilePath);   // Store path of decrypted file  
            }
            }
                catch (Exception ex)
            {
                success = false;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
            }

            // Output the decrypted file paths or empty if unsuccessful  
            DA.SetDataList(0, decryptedFilePaths);
            DA.SetData(1, success); // Output success status  
        }
            // Handle deletion of decrypted files and folders if requested  
        else        
        {
            await Task.Delay(5000); // Delay for 5000 milliseconds (5 seconds)  
            DeleteCreatedFilesAndFoldersAsync();
        }
    }

    private void DecryptFile(string inputFile, string outputFile)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Key; // Use hardcoded key  
            aes.IV = IV;   // Use hardcoded IV  

            using (FileStream fsInput = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            using (FileStream fsDecrypted = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
            using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (CryptoStream csDecrypt = new CryptoStream(fsInput, decryptor, CryptoStreamMode.Read))
            {
                csDecrypt.CopyTo(fsDecrypted);
            }
        }
    }

    private  void DeleteCreatedFilesAndFoldersAsync()
    {
        foreach (var filePath in decryptedFilePaths)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (IOException ioEx)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Failed to delete file: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error during deletion of file: {ex.Message}");
            }
        }

        
        try
        {
            if (Directory.Exists(targetFolderPath))
            {
                Directory.Delete(targetFolderPath, true); // true to delete recursively  
            }
        }
        catch (IOException ioEx)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Failed to delete folder: {ioEx.Message}");
        }
        catch (Exception ex)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error during deletion of folder: {ex.Message}");
        }
        

        // Clear the lists after deletion  
        decryptedFilePaths.Clear();
    }   
}