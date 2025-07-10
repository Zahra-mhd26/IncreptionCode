import base64
from Crypto.Cipher import AES
from Crypto.Util.Padding import pad, unpad
import pandas as pd
import argparse
import io

# Key and IV as defined in the C# project
# IMPORTANT: These must be the same as in the C# code for compatibility
KEY_BASE64 = "GZuHOfU1C5F6SgI+PRKo+g=="
IV_BASE64 = "0v2u51jxM1dwalKzbWiZHg=="

# Decode Key and IV from Base64
KEY = base64.b64decode(KEY_BASE64)
IV = base64.b64decode(IV_BASE64)

def encrypt_excel_data(excel_path, output_path):
    """
    Reads data from an Excel file, encrypts it using AES, and saves it to an output file.
    """
    # Step 1: Read Excel file using pandas
    # Read all sheets if not specified, or a specific sheet
    # For simplicity, this example reads the first sheet.
    # To handle multiple sheets, you might need to serialize them differently,
    # e.g., as a dictionary of DataFrames, then convert to JSON or another format.
    df = pd.read_excel(excel_path, sheet_name=None)

    # Step 2: Serialize the data to bytes
    # We'll convert the dictionary of DataFrames (if multiple sheets) or a single DataFrame
    # to a JSON string, then encode to UTF-8 bytes.
    # Using an in-memory buffer for CSV, then getting bytes
    output_buffer = io.StringIO()
    if isinstance(df, dict): # Multiple sheets
        # For simplicity, let's just take the first sheet's name for CSV
        # A more robust solution would be to save each sheet or combine them
        # or save as JSON which handles multiple structures better.
        # Here, we'll just serialize the first sheet found.
        if df:
            first_sheet_name = list(df.keys())[0]
            df[first_sheet_name].to_csv(output_buffer, index=False)
        else:
            # Handle empty Excel file (no sheets)
            # We can encrypt an empty string or raise an error
            pass # For now, empty buffer means empty data
    else: # Single sheet (df is a DataFrame)
        df.to_csv(output_buffer, index=False)

    data_bytes = output_buffer.getvalue().encode('utf-8')
    output_buffer.close()

    # Step 3: Encrypt the data
    cipher = AES.new(KEY, AES.MODE_CBC, IV)

    # Pad the data to be a multiple of AES block size (16 bytes)
    # The C# code uses CryptoStream which handles padding automatically (PKCS7 is default)
    padded_data = pad(data_bytes, AES.block_size)
    encrypted_data = cipher.encrypt(padded_data)

    # Step 4: Write the encrypted data to the output file
    with open(output_path, 'wb') as f_out:
        f_out.write(encrypted_data)

    print(f"Excel file '{excel_path}' encrypted successfully to '{output_path}'.")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Encrypt an Excel file using AES.")
    parser.add_argument("input_excel", help="Path to the input Excel file (.xlsx)")
    parser.add_argument("output_file", help="Path to save the encrypted output file")

    args = parser.parse_args()

    encrypt_excel_data(args.input_excel, args.output_file)
