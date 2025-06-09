# StandardLicensingGenerator

A Windows desktop tool for generating licenses compatible with the [Standard.Licensing](https://github.com/dnauck/Standard.Licensing) library. The application lets you configure all available license options, sign them with your private key and save the result for distribution.

## Features

- Create **Standard**, **Trial** and **Custom** license types
- Set product name, version and expiration date
- Store customer information (name and email)
- Add any additional attributes using JSON
- Sign licenses using your own RSA private key
- Save generated licenses to a file
- Built-in help screen describing how to use the tool

The UI is designed with sensible defaults and labeled inputs so generating a license only requires filling in the desired fields and selecting your private key.

## Usage

1. Start the application.
2. Enter your product details and customer information.
3. Select the desired license type and expiration date.
4. Optionally add extra attributes in JSON format (e.g. `{ "Seats": "5" }`).
5. Browse to your private key file (XML RSA key used to sign licenses).
6. Click **Generate License** to view the resulting license text.
7. Use **File → Save License** to store the license in a `.lic` file.

The generated license can then be validated in your application using the matching public key with the Standard.Licensing library.

## Distribution

The project targets `net9.0-windows` and can be published as a single-file executable for easy distribution (e.g. via web download). Run `dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true` to produce a distributable package.

## Help

A detailed help window is available from the **Help** menu inside the application. It provides a short overview of the workflow and explains where to place your private/public keys.

