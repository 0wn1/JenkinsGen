# JenkinsGen

**JenkinsGen** is a Windows application designed to generate GTA V hash lists, parse existing ones, and perform reverse lookups and find models from hash. This tool retrieves GTA V Dump Data of objects, vehicles, weapons, and peds models from the [DurtyFree](https://github.com/DurtyFree/gta-v-data-dumps) GitHub repository.

## Preview
- **Preview**: [Streamable](https://streamable.com/mh9d30)

## Features

- **Hash Generation**: Automatically generate hashes for objects, vehicles, weapons, and peds.
- **Hash Lookup**: Look up models by hash and vice versa.
- **Support for Signed and Unsigned Hashes**: Handles both positive and negative hash values.
- **Data Download**: Retrieves and stores GTA V Dump Data into the `hashList.ini` file.
- **Clipboard Support**: Provides buttons to copy text to the clipboard.

## Usage

1. **Running**:
   - Download the compiled version from the [Releases](https://github.com/0wn1/JenkinsGen/releases) page.
   - Extract the zipped file.
   - Run the application.

2. **Generate Hash List**:
   - If the `hashList.ini` file is missing, the application will prompt you to generate it.
   - The tool will download hash lists from DurtyFree's GitHub repository and save them to `hashList.ini`.

3. **Load Known Hashes**:
   - If the `hashList.ini` file exists, the application will load the hashes from it for lookups.

4. **Text Boxes**:
   - **Model**: Enter a string to get its hash.
   - **Hex**: Enter a hexadecimal value to get the hash in different formats.
   - **Unsigned**: Enter a decimal hash value to see it in different formats.
   - **Signed**: Enter a signed hash value to view the corresponding data.

5. **Copy to Clipboard**:
   - Use the clipboard buttons to copy the corresponding values.

## Dependencies

- Windows 7 x64 or later
- [.NET Framework 4.7.2 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472) or later.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact

For issues or questions, please open an issue on the [GitHub repository](https://github.com/0wn1/JenkinsGen/issues).