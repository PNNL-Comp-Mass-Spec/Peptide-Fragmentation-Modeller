Peptide Fragmentation Modeller

This program is a command-line utility reads that reads in a text file of 
peptide sequences and generates the theoretical fragmentation pattern 
for each using the VB.NET DLL version of the Molecular Weight Calculator
Results are reported as a single concatenated DTA file, or in 
separate .Dta files.  The ion intensity values are not predicted values; 
instead, b, c, y, and z ions are assigned an intensity of 100; while a ions 
and neutral loss ions receive an intensity of 20.  

You can customize the ions that are included in the theoretical spectra using
either an XML parameter file, or using command line switches.

To run:
1) Open a command prompt window
2) Enter the path that points to the program (e.g. cd c:\Program Files\PeptideFragmentationModeller\
    for typical installation)
3) Enter the executable filename (i.e. PeptideFragmentationModeller.exe), then a 
   space, then /I: followed by the name of the file containing peptides to process
4) Wait until the program is finished.

Note: Running the program with no input file path will open a dialog with further 
options available to the user. Double clicking on the program in an Explorer-style 
window will also show the user options, but the program will terminate upon 
closing the dialog, so the command line option outlined above is still required.

Note that the _DTA.txt file can be converted to a Mascot Generic Format (MGF) file
using the Concatenated DTA to Mascot Generic File (MGF) File Converter application,
available at http://omics.pnl.gov/software

Program syntax:
PeptideFragmentationModeller.exe
 InputFilePath.txt [/O:OutputFolderName [/P:ParameterFilePath]]
 [/Double:[MZThreshold]] [/Triple:[MZThreshold]]
 [/A] [/B] [/C] [/Y] [/Z] [/ETD]
 [/NLWater] [/NLAmmonia] [/NLPhosphate] [/IonShoulder]
 [/Label:[Verbose]] [/DTA] [/CDTA:[FileName]] [/Over]

The input file should have one peptide per line.
The output folder switch is optional.  If omitted, the DTA files will be created in the same folder as the input file.
The parameter file path is optional.  If included, it should point to a valid XML parameter file.

By default, will show B and Y ions.  Use /A through /Z to control the ions to show.  For example, to include A ions then
 use /A.  To hide B ions, use /B:False
The /ETD switch is shorthand for /A:False /B:False /Y:False /C /Z

Neutral loss ions can be shown using /NLWater, /NLAmmonia, or /NLPhosphate.
Ion shoulder ions can be shown using /IonShoulder (shoulder ions are spaced 1 m/z away from each b, y, c, or z ion, but
have 50% the intensity)

Use /Double to also include doubly charged (2+) peaks for ions over 800 m/z.  You can customize this m/z threshold to a
different m/z, for example 850 m/z, using /Double:850

Similarly, use /Triple to include 3+ peaks for ions over 900 m/z.  You can customize this m/z threshold to a different 
m/z, say 1200 m/z, using /Triple:1200

If you use /Label, then generic ion labels will be included in the output .DTA files.  Use /Label:Verbose to get 
detailed ion labels (like b3 or y5)

By default, will create a single _DTA.txt file (aka concatenated DTA file); default name is ModelSpectra_dta.txt. 
To specify the filename, use /CDTA:OutputFile_dta.txt
To create a separate DTA file for each peptide, use the /DTA switch
Use /Over or /Overwrite to overwrite existing .DTA files

Modified residues can be specified using modification symbols (see below for defaults)
For example: 
  VPTPNVSVVDLTC!RLEK

Use /Mods:ModList to define custom modification symbols.  Enter the symbols as a semicolon separated list using
the format /Mods:ModSymbol1=ModMass1;ModSymbol2=ModMass2;ModSymbol3=ModMass3
For example: /Mods:+=14.01565;@=15.99492

Always use the * symbol for phosphorylation (phosphorylated residues will get neutral loss peaks created for 
them if switch NLPhosphate is used)

Default modification symbols are:

Symbol    Mass        Description
*         79.96633    Phosphorylation [HPO3]
+         14.01565    Methylation [CH2]
@         15.99492    Oxidation [O]
!         57.02146    Carbamidomethylation [C2H3NO]
&         58.00548    Carboxymethylation [CH2CO2]
#         71.03711    Acrylamide [CHCH2CONH2]
$         227.127     Cleavable ICAT [(^12C10)H17N3O3]
%         236.127     Cleavable ICAT [(^13C9)(^12C)H17N3O3]
~         442.225     ICAT D0 [C20H34N4O5S]
`         450.274     ICAT D8 [C20H26D8N4O5S]

Use /L to specify that a log file should be created.  
Use /L:LogFilePath to specify the name (or full path) for the log file.

-------------------------------------------------------------------------------
Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)

E-mail: matthew.monroe@pnnl.gov or matt@alchemistmatt.com
Website: http://omics.pnl.gov/ or http://www.sysbio.org/resources/staff/
-------------------------------------------------------------------------------

Licensed under the Apache License, Version 2.0; you may not use this file except 
in compliance with the License.  You may obtain a copy of the License at 
http://www.apache.org/licenses/LICENSE-2.0

All publications that result from the use of this software should include 
the following acknowledgment statement:
 Portions of this research were supported by the W.R. Wiley Environmental 
 Molecular Science Laboratory, a national scientific user facility sponsored 
 by the U.S. Department of Energy's Office of Biological and Environmental 
 Research and located at PNNL.  PNNL is operated by Battelle Memorial Institute 
 for the U.S. Department of Energy under contract DE-AC05-76RL0 1830.

Notice: This computer software was prepared by Battelle Memorial Institute, 
hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the 
Department of Energy (DOE).  All rights in the computer software are reserved 
by DOE on behalf of the United States Government and the Contractor as 
provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY 
WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS 
SOFTWARE.  This notice including this sentence must appear on any copies of 
this computer software.
