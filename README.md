# Svg-To-Xaml-converter
A console app  to convert SVG simplified icons to XAML format


## How to use
Copy the exe file to the folder where the svg files are located.
You can convert a number of SVG files or all the SVG files in a folder.


At the command prompt type `SvgToXaml filename1.svg [filename2.svg]... [filenameN.svg]` to convert individual icons.

At the command prompt type `SvgToXaml /all` to convert all svg icons in current folder.

At the command prompt type `SvgToXaml /help` for usage help.

**Notes:** 
* Only one path allowed in svg
* Color is allowed only as a svg fill attribute of the following formats: 
  * `style="fill: #xxxxxx;"` or 
  * `style="fill: rgb(xxx, xxx, xxx);"`
* **XAML FILES WILL BE OVERWRITEN**


####This code is supplied AS IS. We don't provide support for this application. Feel free to comment, file issues and send PRs with improvements.
