Option Strict On

' See clsPeptideFragmentationModeller for the program description

' -------------------------------------------------------------------------------
' Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
' Program started November 9, 2003
'
' E-mail: matthew.monroe@pnl.gov or matt@alchemistmatt.com
' Website: http://ncrr.pnl.gov/ or http://www.sysbio.org/resources/staff/
' -------------------------------------------------------------------------------
' 
'
' Example command line:
'  InputFilePath.txt /o:OutputFolderPath

Imports System.Collections.Generic

Module modMain

	Public Const PROGRAM_DATE As String = "April 25, 2014"

	Private mInputFilePath As String
	Private mOutputFolderPath As String				' Optional
	Private mParameterFilePath As String			' Optional
	''Private mOutputFolderAlternatePath As String    ' Optional

	Private mLogMessagesToFile As Boolean
	Private mLogFilePath As String = String.Empty

	Private mQuietMode As Boolean

	Private mShowAIons As Boolean
	Private mShowBIons As Boolean
	Private mShowCIons As Boolean
	Private mShowYIons As Boolean
	Private mShowZIons As Boolean

	Private mNeutralLossAmmonia As Boolean
	Private mNeutralLossPhosphate As Boolean
	Private mNeutralLossWater As Boolean

	Private mIncludeShoulderIons As Boolean

	Private mIncludeDoublyChargedIons As Boolean
	Private mDoubleChargeMZThreshold As Single

	Private mIncludeTriplyChargedIons As Boolean
	Private mTripleChargeMZThreshold As Single

	Private mLabelIons As Boolean
	Private mLabelIonsVerbose As Boolean

	Private mConcatenatedDTA As Boolean
	Private mConcatenatedDTAFileName As String

	Private mOverwriteFiles As Boolean

	Private mCustomModSymbols As Dictionary(Of Char, Double)

	Private mShowMassValues As Boolean

	Private mMassValuesFilePath As String

	Private WithEvents mPeptideFragmentationModeller As clsPeptideFragmentationModeller
	Private mLastProgressReportTime As DateTime
	Private mLastProgressReportValue As Integer

	Public Function Main() As Integer
		' Returns 0 if no error, error code if an error

		Dim intReturnCode As Integer
		Dim objParseCommandLine As New clsParseCommandLine
		Dim blnProceed As Boolean

		intReturnCode = 0
		mInputFilePath = String.Empty
		mOutputFolderPath = String.Empty
		mParameterFilePath = String.Empty

		mQuietMode = False
		mLogMessagesToFile = False
		mLogFilePath = String.Empty

		mShowAIons = False
		mShowBIons = True
		mShowCIons = False
		mShowYIons = True
		mShowZIons = False

		mNeutralLossAmmonia = False
		mNeutralLossPhosphate = False
		mNeutralLossWater = False

		mIncludeShoulderIons = False

		mIncludeDoublyChargedIons = False
		mDoubleChargeMZThreshold = MwtWinDll.MWPeptideClass.DEFAULT_DOUBLE_CHARGE_MZ_THRESHOLD

		mIncludeTriplyChargedIons = False
		mTripleChargeMZThreshold = MwtWinDll.MWPeptideClass.DEFAULT_TRIPLE_CHARGE_MZ_THRESHOLD

		mLabelIons = False
		mLabelIonsVerbose = False

		mConcatenatedDTA = True
		mConcatenatedDTAFileName = clsPeptideFragmentationModeller.DEFAULT_CONCATENATED_DTA_FILENAME

		mOverwriteFiles = False

		mCustomModSymbols = New Dictionary(Of Char, Double)

		mShowMassValues = False

		mMassValuesFilePath = String.Empty

		Try
			blnProceed = False

			If objParseCommandLine.ParseCommandLine Then
				If SetOptionsUsingCommandLineParameters(objParseCommandLine) Then blnProceed = True
			End If

			If mShowMassValues Then
				mPeptideFragmentationModeller = New clsPeptideFragmentationModeller
				mPeptideFragmentationModeller.ExportMassValues(mMassValuesFilePath)
				Return 0
			End If

			If Not blnProceed OrElse _
			   objParseCommandLine.NeedToShowHelp OrElse _
			   objParseCommandLine.ParameterCount + objParseCommandLine.NonSwitchParameterCount = 0 OrElse _
			   mInputFilePath.Length = 0 Then
				ShowProgramHelp()
				intReturnCode = -1
			Else

				mPeptideFragmentationModeller = New clsPeptideFragmentationModeller

				With mPeptideFragmentationModeller
					.ShowMessages = Not mQuietMode
					.LogMessagesToFile = mLogMessagesToFile
					.LogFilePath = mLogFilePath

					.ShowAIons = mShowAIons
					.ShowBIons = mShowBIons
					.ShowCIons = mShowCIons
					.ShowYIons = mShowYIons
					.ShowZIons = mShowZIons

					.NeutralLossAmmonia = mNeutralLossAmmonia
					.NeutralLossPhosphate = mNeutralLossPhosphate
					.NeutralLossWater = mNeutralLossWater

					.IncludeShoulderIons = mIncludeShoulderIons

					.IncludeDoublyChargedIons = mIncludeDoublyChargedIons
					.DoubleChargeMZThreshold = mDoubleChargeMZThreshold

					.IncludeTriplyChargedIons = mIncludeTriplyChargedIons
					.TripleChargeMZThreshold = mTripleChargeMZThreshold

					.LabelIons = mLabelIons
					.LabelIonsVerbose = mLabelIonsVerbose

					.ConcatenatedDTA = mConcatenatedDTA
					.ConcatenatedDTAFileName = mConcatenatedDTAFileName

					.OverwriteExistingFiles = mOverwriteFiles

					If mCustomModSymbols.Count > 0 Then
						.CustomModSymbols = mCustomModSymbols
					End If
				End With


				If mPeptideFragmentationModeller.ProcessFilesWildcard(mInputFilePath, mOutputFolderPath, mParameterFilePath) Then
					intReturnCode = 0
				Else
					intReturnCode = -1
					If intReturnCode <> 0 AndAlso Not mQuietMode Then
						Console.WriteLine("Error while processing: " & mPeptideFragmentationModeller.GetErrorMessage())
					End If
				End If

			End If

		Catch ex As Exception
			Console.WriteLine("Error occurred in modMain->Main: " & ControlChars.NewLine & ex.Message)
			intReturnCode = -1
		End Try

		Return intReturnCode

	End Function

	Private Sub DisplayProgressPercent(ByVal intPercentComplete As Integer, ByVal blnAddCarriageReturn As Boolean)
		If blnAddCarriageReturn Then
			Console.WriteLine()
		End If
		If intPercentComplete > 100 Then intPercentComplete = 100
		Console.Write("Processing: " & intPercentComplete.ToString & "% ")
		If blnAddCarriageReturn Then
			Console.WriteLine()
		End If
	End Sub

	Private Function GetAppVersion() As String
		'Return Windows.Forms.Application.ProductVersion & " (" & PROGRAM_DATE & ")"

		Return Reflection.Assembly.GetExecutingAssembly.GetName.Version.ToString & " (" & PROGRAM_DATE & ")"
	End Function

	''' <summary>
	''' Parse the mod symbol list, splitting first on semicolon and then on an equals sign
	''' </summary>
	''' <param name="strModSymbolList"></param>
	''' <returns>True if success; false if a problem</returns>
	''' <remarks></remarks>
	Private Function ParseModSymbolList(ByVal strModSymbolList As String) As Boolean
		Dim strModDefs() As String
		Dim strDetails() As String
		Dim blnShowHelp As Boolean

		Dim chModSymbol As Char
		Dim dblModMass As Double

		Try
			If String.IsNullOrEmpty(strModSymbolList) Then Exit Function

			strModDefs = strModSymbolList.Split(";"c)

			If mCustomModSymbols Is Nothing Then
				mCustomModSymbols = New Dictionary(Of Char, Double)
			Else
				mCustomModSymbols.Clear()
			End If

			For Each strModDef As String In strModDefs
				If Not String.IsNullOrEmpty(strModDef) Then
					strDetails = strModDef.Split("="c)

					If strDetails.Length < 2 Then
						Console.WriteLine("Invalid modification definition: " & strModDef)
						blnShowHelp = True
					Else
						chModSymbol = strDetails(0).Chars(0)
						If Double.TryParse(strDetails(1), dblModMass) Then
							If mCustomModSymbols.ContainsKey(chModSymbol) Then
								Console.WriteLine("Modification symbol " & chModSymbol & " already defined; skipping duplicate definition: " & strModDef)
								blnShowHelp = True
							Else
								mCustomModSymbols.Add(chModSymbol, dblModMass)
							End If
						Else
							Console.WriteLine("Error parsing out modification mass from modification definition: " & strModDef & "; did not find a number after the equals sign")
							blnShowHelp = True
						End If
					End If

				End If
			Next
		Catch ex As Exception
			Console.WriteLine("Error parsing the mod symbol list: " & ControlChars.NewLine & ex.Message)
			blnShowHelp = True
		End Try

		If blnShowHelp Then
			Console.WriteLine()
			Console.WriteLine("To define two modifications using symbols + and @ with masses 14.01565 and 15.99492, use:")
			Console.WriteLine("  /Mods:+=14.01565;@=15.99492")
			Console.WriteLine()
			Return False
		Else
			Return True
		End If

	End Function

	Private Function SetOptionsUsingCommandLineParameters(ByVal objParseCommandLine As clsParseCommandLine) As Boolean
		' Returns True if no problems; otherwise, returns false

		Dim strValue As String = String.Empty
		Dim strValidParameters() As String = New String() {"I", "O", "P", "Double", "Triple", "A", "B", "C", "Y", "Z", _
			  "NLWater", "NLAmmonia", "NLPhosphate", "IonShoulder", "Shoulder", "ETD", "Label", _
			  "Overwrite", "Over", "DTA", "CDTA", "Mods", "L", "MassValues"}
		Dim sngValue As Single

		Try
			' Make sure no invalid parameters are present 
			If objParseCommandLine.InvalidParametersPresent(strValidParameters) Then
				Return False
			Else
				With objParseCommandLine
					' Query objParseCommandLine to see if various parameters are present
					If .RetrieveValueForParameter("I", strValue) Then
						mInputFilePath = strValue
					ElseIf .NonSwitchParameterCount > 0 Then
						mInputFilePath = .RetrieveNonSwitchParameter(0)
					End If

					If .RetrieveValueForParameter("O", strValue) Then mOutputFolderPath = strValue
					If .RetrieveValueForParameter("P", strValue) Then mParameterFilePath = strValue

					If .RetrieveValueForParameter("Q", strValue) Then mQuietMode = True

					If .RetrieveValueForParameter("Double", strValue) Then
						mIncludeDoublyChargedIons = ValueToBool(strValue, True)
						If Single.TryParse(strValue, sngValue) Then
							mDoubleChargeMZThreshold = sngValue
						End If
					End If

					If .RetrieveValueForParameter("Triple", strValue) Then
						mIncludeTriplyChargedIons = ValueToBool(strValue, True)
						If Single.TryParse(strValue, sngValue) Then
							mTripleChargeMZThreshold = sngValue
						End If
					End If

					If .RetrieveValueForParameter("A", strValue) Then mShowAIons = ValueToBool(strValue, True)
					If .RetrieveValueForParameter("B", strValue) Then mShowBIons = ValueToBool(strValue, True)
					If .RetrieveValueForParameter("C", strValue) Then mShowCIons = ValueToBool(strValue, True)
					If .RetrieveValueForParameter("Y", strValue) Then mShowYIons = ValueToBool(strValue, True)
					If .RetrieveValueForParameter("Z", strValue) Then mShowZIons = ValueToBool(strValue, True)

					If .RetrieveValueForParameter("NLWater", strValue) Then mNeutralLossWater = ValueToBool(strValue, True)
					If .RetrieveValueForParameter("NLAmmonia", strValue) Then mNeutralLossAmmonia = ValueToBool(strValue, True)
					If .RetrieveValueForParameter("NLPhosphate", strValue) Then mNeutralLossPhosphate = ValueToBool(strValue, True)

					If .RetrieveValueForParameter("Shoulder", strValue) Then mIncludeShoulderIons = ValueToBool(strValue, True)
					If .RetrieveValueForParameter("IonShoulder", strValue) Then mIncludeShoulderIons = ValueToBool(strValue, True)

					If .RetrieveValueForParameter("ETD", strValue) Then
						mShowAIons = False
						mShowBIons = False
						mShowCIons = True
						mShowYIons = False
						mShowZIons = True
					End If

					If .RetrieveValueForParameter("Label", strValue) Then
						mLabelIons = True
						If Not String.IsNullOrEmpty(strValue) Then
							If strValue.ToLower.StartsWith("v") Then
								mLabelIonsVerbose = True
							End If
						End If
					End If

					If .RetrieveValueForParameter("Over", strValue) Then mOverwriteFiles = True
					If .RetrieveValueForParameter("Overwrite", strValue) Then mOverwriteFiles = True

					If .RetrieveValueForParameter("DTA", strValue) Then mConcatenatedDTA = False

					If .RetrieveValueForParameter("CDTA", strValue) Then
						mConcatenatedDTA = True
						If Not String.IsNullOrEmpty(strValue) Then
							mConcatenatedDTAFileName = strValue
						End If
					End If

					If .RetrieveValueForParameter("Mods", strValue) Then
						If Not ParseModSymbolList(strValue) Then
							Return False
						End If
					End If

					If .RetrieveValueForParameter("L", strValue) Then
						mLogMessagesToFile = True

						If Not strValue Is Nothing AndAlso strValue.Length > 0 Then
							mLogFilePath = strValue.Trim(""""c)
						End If
					End If

					If .RetrieveValueForParameter("MassValues", strValue) Then
						mShowMassValues = True
						If Not String.IsNullOrEmpty(strValue) Then
							mMassValuesFilePath = strValue
						End If
					End If


				End With

				Return True
			End If

		Catch ex As Exception
			Console.WriteLine("Error parsing the command line parameters: " & ControlChars.NewLine & ex.Message)
		End Try

	End Function

	Private Sub ShowProgramHelp()

		Try

			Console.WriteLine("This program reads in a text file of peptide sequences and generates the theoretical fragmentation pattern for each, outputing the results in .Dta files.")
			Console.WriteLine()
			Console.WriteLine("Program syntax:" & ControlChars.NewLine & IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location))
			Console.WriteLine(" InputFilePath.txt [/O:OutputFolderName [/P:ParameterFilePath]]")
			Console.WriteLine(" [/Double:[MZThreshold]] [/Triple:[MZThreshold]]")
			Console.WriteLine(" [/A] [/B] [/C] [/Y] [/Z] [/ETD]")
			Console.WriteLine(" [/NLWater] [/NLAmmonia] [/NLPhosphate] [/IonShoulder]")
			Console.WriteLine(" [/Label[:Verbose]] [/DTA] [/CDTA:[FileName]] [/Over]")
			Console.WriteLine(" [/Mods:ModList] [/L:[LogFilePath]] [/MassValues[:FileName]]")
			Console.WriteLine()
			Console.WriteLine("The input file should have one peptide per line.")
			Console.WriteLine("The output folder switch is optional.  If omitted, the DTA files will be created in the same folder as the input file.")
			Console.WriteLine("The parameter file path is optional.  If included, it should point to a valid XML parameter file.")
			Console.WriteLine()
			Console.WriteLine("By default, will show B and Y ions.  Use /A through /Z to control the ions to show.  For example, to include A ions then use /A.  To hide B ions, use /B:False")
			Console.WriteLine("The /ETD switch is shorthand for /A:False /B:False /Y:False /C /Z")
			Console.WriteLine()
			Console.WriteLine("Neutral loss ions can be shown using /NLWater, /NLAmmonia, or /NLPhosphate.")
			Console.WriteLine("Ion shoulder ions can be shown using /IonShoulder (shoulder ions are spaced 1 m/z away from each b, y, c, or z ion, but have 50% the intensity)")
			Console.WriteLine()
			Console.WriteLine("Use /Double to also include doubly charged (2+) peaks for ions over " & MwtWinDll.MWPeptideClass.DEFAULT_DOUBLE_CHARGE_MZ_THRESHOLD.ToString("0") & " m/z.  You can customize this m/z threshold to a different m/z, for example 850 m/z, using /Double:850")
			Console.WriteLine()
			Console.WriteLine("Similarly, use /Triple to include 3+ peaks for ions over " & MwtWinDll.MWPeptideClass.DEFAULT_TRIPLE_CHARGE_MZ_THRESHOLD.ToString("0") & " m/z.  You can customize this m/z threshold to a different m/z, say 1200 m/z, using /Triple:1200")
			Console.WriteLine()
			Console.WriteLine("If you use /Label, then generic ion labels will be included in the output .DTA files.  Use /Label:Verbose to get detailed ion labels (like b3 or y5)")
			Console.WriteLine()
			Console.WriteLine("By default, will create a single _DTA.txt file (aka concatenated DTA file); default name is " & clsPeptideFragmentationModeller.DEFAULT_CONCATENATED_DTA_FILENAME & ". To specify the filename, use /CDTA:OutputFile_dta.txt")
			Console.WriteLine("To create a separate DTA file for each peptide, use the /DTA switch")
			Console.WriteLine("Use /Over or /Overwrite to overwrite existing .DTA files")
			Console.WriteLine()
			Console.WriteLine("Modified residues can be specified using modification symbols, for example: VPTPNVS*VVDLTC!RLEK")
			Console.WriteLine("! is 57.02146 and * is 79.96633; see the Readme.txt file for default mod symbols and masses")
			Console.WriteLine("Use /Mods:ModList to define custom modification symbols.  Enter the symbols as a semicolon separated list using " & _
							  "the format /Mods:ModSymbol1=ModMass1;ModSymbol2=ModMass2;ModSymbol3=ModMass3")
			Console.WriteLine("For example: /Mods:+=14.01565;@=15.99492")
			Console.WriteLine("Always use the * symbol for phosphorylation (phosphorylated residues will get neutral loss peaks created for them if switch NLPhosphate is used)")
			Console.WriteLine()
			Console.WriteLine("Use /MassValues to display a list of the mass values used by this software (C, H, N, O, and S, plus also the amino acid masses")
			Console.WriteLine("Use /MassValues:FileName to export the list of mass values to a tab-delimited text file")
			Console.WriteLine()
			Console.WriteLine("Use /L to specify that a log file should be created.  Use /L:LogFilePath to specify the name (or full path) for the log file.")
			Console.WriteLine()

			' Console.WriteLine("Use /S to process all valid files in the input folder and subfolders. Include a number after /S (like /S:2) to limit the level of subfolders to examine.")
			' Console.WriteLine("When using /S, you can redirect the output of the results using /A.")
			' Console.WriteLine("When using /S, you can use /R to re-create the input folder hierarchy in the alternate output folder (if defined).")

			Console.WriteLine("Program written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2010")
			Console.WriteLine("Version: " & GetAppVersion())
			Console.WriteLine()

			Console.WriteLine("E-mail: matthew.monroe@pnnl.gov or matt@alchemistmatt.com")
			Console.WriteLine("Website: http://omics.pnl.gov/ or http://www.sysbio.org/resources/staff/")

			' Delay for 750 msec in case the user double clicked this file from within Windows Explorer (or started the program via a shortcut)
			Threading.Thread.Sleep(750)

		Catch ex As Exception
			Console.WriteLine("Error displaying the program syntax: " & ex.Message)
		End Try

	End Sub

	Private Function ValueToBool(ByVal strValue As String, ByVal blnDefaultValue As Boolean) As Boolean
		Dim blnValue As Boolean

		Try
			blnValue = blnDefaultValue

			If Not String.IsNullOrEmpty(strValue) Then
				strValue = strValue.ToLower
				Select Case strValue
					Case "off", "no", "f"
						strValue = "false"
					Case "on", "yes", "t"
						strValue = "true"
				End Select

				If Not Boolean.TryParse(strValue, blnValue) Then
					blnValue = blnDefaultValue
				End If

			End If
		Catch ex As Exception
			blnValue = blnDefaultValue
		End Try

		Return blnValue
	End Function

	Private Sub mPeptideFragmentationModeller_ProgressChanged(ByVal taskDescription As String, ByVal percentComplete As Single) Handles mPeptideFragmentationModeller.ProgressChanged
		Const PERCENT_REPORT_INTERVAL As Integer = 25
		Const PROGRESS_DOT_INTERVAL_MSEC As Integer = 250

		If percentComplete >= mLastProgressReportValue Then
			If mLastProgressReportValue > 0 Then
				Console.WriteLine()
			End If
			DisplayProgressPercent(mLastProgressReportValue, False)
			mLastProgressReportValue += PERCENT_REPORT_INTERVAL
			mLastProgressReportTime = DateTime.UtcNow
		Else
			If DateTime.UtcNow.Subtract(mLastProgressReportTime).TotalMilliseconds > PROGRESS_DOT_INTERVAL_MSEC Then
				mLastProgressReportTime = DateTime.UtcNow
				Console.Write(".")
			End If
		End If
	End Sub

	Private Sub mPeptideFragmentationModeller_ProgressReset() Handles mPeptideFragmentationModeller.ProgressReset
		mLastProgressReportTime = DateTime.UtcNow
		mLastProgressReportValue = 0
	End Sub

End Module
