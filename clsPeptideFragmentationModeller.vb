Option Explicit On 

Imports System.Collections.Generic
Imports System.IO

Public Class clsPeptideFragmentationModeller

    ' Written by Matthew Monroe and Niksa Blonder for the Department of Energy (PNNL, Richland, WA)
    ' Copyright 2005, Battelle Memorial Institute.  All Rights Reserved.
    ' Started March 22, 2005
    '

    Inherits clsProcessFilesBaseClass

    Public Sub New()
		MyBase.mFileDate = "June 19, 2013"
        InitializeVariables()
    End Sub


#Region "Constants and Enums"

    Public Const DEFAULT_CONCATENATED_DTA_FILENAME As String = "ModelSpectra_dta.txt"

    Public Enum ePeptideFragmentationModellerErrorCodes
        NoError = 0
        InputFileAccessError = 1
        UnspecifiedError = -1
    End Enum

#End Region

#Region "Structures"
    ' No structures yet
#End Region

#Region "Classwide Variables"

    Private mConcatenatedDTA As Boolean
    Private mConcatenatedDTAFileName As String

    Private mOverwriteExistingFiles As Boolean

    Private mLocalErrorCode As ePeptideFragmentationModellerErrorCodes

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
    Private mDoubleChargeMZThreshold As Single = MwtWinDll.MWPeptideClass.DEFAULT_DOUBLE_CHARGE_MZ_THRESHOLD

    Private mIncludeTriplyChargedIons As Boolean
    Private mTripleChargeMZThreshold As Single = MwtWinDll.MWPeptideClass.DEFAULT_TRIPLE_CHARGE_MZ_THRESHOLD

    Private mLabelIons As Boolean
    Private mLabelIonsVerbose As Boolean

	Private mCustomModSymbols As Dictionary(Of Char, Double)

#End Region

#Region "Processing Options Interface Functions"

    Public ReadOnly Property LocalErrorCode() As ePeptideFragmentationModellerErrorCodes
        Get
            Return mLocalErrorCode
        End Get
    End Property

    Public Property ConcatenatedDTA() As Boolean
        Get
            Return mConcatenatedDTA
        End Get
        Set(ByVal value As Boolean)
            mConcatenatedDTA = value
        End Set
    End Property

    Public Property ConcatenatedDTAFileName() As String
        Get
            Return mConcatenatedDTAFileName
        End Get
        Set(ByVal value As String)
            If String.IsNullOrEmpty(value) Then
                mConcatenatedDTAFileName = DEFAULT_CONCATENATED_DTA_FILENAME
            Else
                mConcatenatedDTAFileName = value
            End If
        End Set
    End Property

	Public Property CustomModSymbols() As Dictionary(Of Char, Double)
		Get
			Return mCustomModSymbols
		End Get
		Set(ByVal value As Dictionary(Of Char, Double))
			mCustomModSymbols = value
		End Set
	End Property

    Public Property LabelIons() As Boolean
        Get
            Return mLabelIons
        End Get
        Set(ByVal value As Boolean)
            mLabelIons = value
        End Set
    End Property

    Public Property LabelIonsVerbose() As Boolean
        Get
            Return mLabelIonsVerbose
        End Get
        Set(ByVal value As Boolean)
            mLabelIonsVerbose = value
        End Set
    End Property

    Public Property OverwriteExistingFiles() As Boolean
        Get
            Return mOverwriteExistingFiles
        End Get
        Set(ByVal Value As Boolean)
            mOverwriteExistingFiles = Value
        End Set
    End Property

    Public Property ShowAIons() As Boolean
        Get
            Return mShowAIons
        End Get
        Set(ByVal value As Boolean)
            mShowAIons = value
        End Set
    End Property

    Public Property ShowBIons() As Boolean
        Get
            Return mShowBIons
        End Get
        Set(ByVal value As Boolean)
            mShowBIons = value
        End Set
    End Property

    Public Property ShowCIons() As Boolean
        Get
            Return mShowCIons
        End Get
        Set(ByVal value As Boolean)
            mShowCIons = value
        End Set
    End Property

    Public Property ShowYIons() As Boolean
        Get
            Return mShowYIons
        End Get
        Set(ByVal value As Boolean)
            mShowYIons = value
        End Set
    End Property

    Public Property ShowZIons() As Boolean
        Get
            Return mShowZIons
        End Get
        Set(ByVal value As Boolean)
            mShowZIons = value
        End Set
    End Property

    Public Property NeutralLossAmmonia() As Boolean
        Get
            Return mNeutralLossAmmonia
        End Get
        Set(ByVal value As Boolean)
            mNeutralLossAmmonia = value
        End Set
    End Property

    Public Property NeutralLossPhosphate() As Boolean
        Get
            Return mNeutralLossPhosphate
        End Get
        Set(ByVal value As Boolean)
            mNeutralLossPhosphate = value
        End Set
    End Property

    Public Property NeutralLossWater() As Boolean
        Get
            Return mNeutralLossWater
        End Get
        Set(ByVal value As Boolean)
            mNeutralLossWater = value
        End Set
    End Property

    Public Property IncludeShoulderIons() As Boolean
        Get
            Return mIncludeShoulderIons
        End Get
        Set(ByVal value As Boolean)
            mIncludeShoulderIons = value
        End Set
    End Property

    Public Property IncludeDoublyChargedIons() As Boolean
        Get
            Return mIncludeDoublyChargedIons
        End Get
        Set(ByVal value As Boolean)
            mIncludeDoublyChargedIons = value
        End Set
    End Property

    Public Property DoubleChargeMZThreshold() As Single
        Get
            Return mDoubleChargeMZThreshold
        End Get
        Set(ByVal value As Single)
            If value < 0 Then
                mDoubleChargeMZThreshold = 0
            Else
                mDoubleChargeMZThreshold = value
            End If
        End Set
    End Property

    Public Property IncludeTriplyChargedIons() As Boolean
        Get
            Return mIncludeTriplyChargedIons
        End Get
        Set(ByVal value As Boolean)
            mIncludeTriplyChargedIons = value
        End Set
    End Property

    Public Property TripleChargeMZThreshold() As Single
        Get
            Return mTripleChargeMZThreshold
        End Get
        Set(ByVal value As Single)
            If value < 0 Then
                mTripleChargeMZThreshold = 0
            Else
                mTripleChargeMZThreshold = value
            End If
        End Set
    End Property

#End Region

    Private Sub CreateDtaFile(ByVal strDtaFileName As String, ByVal udtFragSpectrum() As MwtWinDll.MWPeptideClass.udtFragmentationSpectrumDataType, ByVal dblMH As Double, ByVal charge As Integer)

        ' Create a .dta file and write out the masses and intensity values in ascending order
        ' Only write out the ions that have .SymbolGeneric must be "b" or "y"

		Dim ioFileInfo As FileInfo
		Dim swOutFile As StreamWriter

		Dim sbLineOut As New Text.StringBuilder

		Dim intIndex As Integer

		Dim FileName As String

		Dim dblMaxIntensity As Double
		Dim strLabel As String
		Dim strExistingLabel As String = String.Empty

		Dim blnIntensityUpdated As Boolean

		Dim objIonsForMass As SortedList(Of Double, String) = Nothing

		Dim blnProceed As Boolean

		Dim objData = New SortedList(Of Double, SortedList(Of Double, String))

		Try
			For intIndex = 0 To udtFragSpectrum.Length - 1

				If udtFragSpectrum(intIndex).Mass > 0 Then

					If mLabelIonsVerbose Then
						strLabel = udtFragSpectrum(intIndex).Symbol
					Else
						strLabel = udtFragSpectrum(intIndex).SymbolGeneric
					End If

					If objData.TryGetValue(udtFragSpectrum(intIndex).Mass, objIonsForMass) Then
						' Mass already exists
						' Update objIonsForMass

						If objIonsForMass.TryGetValue(udtFragSpectrum(intIndex).Intensity, strExistingLabel) Then
							strExistingLabel &= ", " & strLabel
							objIonsForMass(udtFragSpectrum(intIndex).Intensity) = strExistingLabel
						Else
							objIonsForMass.Add(udtFragSpectrum(intIndex).Intensity, strLabel)
						End If

					Else
						objIonsForMass = New SortedList(Of Double, String)

						objIonsForMass.Add(udtFragSpectrum(intIndex).Intensity, strLabel)
						objData.Add(udtFragSpectrum(intIndex).Mass, objIonsForMass)

					End If

				End If
			Next

		Catch ex As Exception
			HandleException("Error populating sngMZ and sngIntensity in CreateDtaFile", ex)
		End Try

		FileName = strDtaFileName & "." & CStr(charge) & ".dta"

		If mConcatenatedDTA Then
			blnProceed = True
		Else
			ioFileInfo = New FileInfo(FileName)

			If ioFileInfo.Exists() Then
				If mOverwriteExistingFiles Then
					blnProceed = True
				Else
					ShowMessage("Skipping existing file: " & ioFileInfo.Name)
					blnProceed = False
				End If
			Else
				blnProceed = True
			End If
		End If

		If blnProceed Then

			' Use the following to write out the data to the output ifle

			If mConcatenatedDTA Then
				swOutFile = New StreamWriter(New FileStream(mConcatenatedDTAFileName, FileMode.Append, FileAccess.Write, FileShare.Read))

				' Write the header lines
				swOutFile.WriteLine()
				swOutFile.WriteLine("=================================== """ & Path.GetFileName(FileName) & """ ==================================")
			Else
				swOutFile = New StreamWriter(FileName)
			End If

			swOutFile.WriteLine(Math.Round(dblMH, 4).ToString & " " & charge.ToString)

			For intIndex = 0 To objData.Count - 1

				objIonsForMass = objData.Values(intIndex)

				dblMaxIntensity = 0
				strLabel = String.Empty

				For intIonIndex As Integer = 0 To objIonsForMass.Count - 1
					blnIntensityUpdated = False
					If objIonsForMass.Keys(intIonIndex) > dblMaxIntensity Then
						dblMaxIntensity = objIonsForMass.Keys(intIonIndex)
						blnIntensityUpdated = True
					End If

					If String.IsNullOrEmpty(strLabel) Then
						strLabel = objIonsForMass.Values(intIonIndex)
					Else
						If blnIntensityUpdated Then
							strLabel = objIonsForMass.Values(intIonIndex) & ", " & strLabel
						Else
							strLabel &= ", " & objIonsForMass.Values(intIonIndex)
						End If
					End If

				Next

				If String.IsNullOrEmpty(strLabel) Then strLabel = String.Empty

				sbLineOut.Length = 0
				sbLineOut.Append(CStr(objData.Keys(intIndex)) & " " & CStr(dblMaxIntensity))

				If mLabelIons OrElse mLabelIonsVerbose Then

					' Pad strOutLine to a length of 15 characters
					Do While sbLineOut.Length < 15
						sbLineOut.Append(" ")
					Loop

					' Now add the label
					sbLineOut.Append(strLabel)
				End If

				swOutFile.WriteLine(sbLineOut.ToString)

			Next

			If Not mConcatenatedDTA Then
				' Add a blank line
				swOutFile.WriteLine()
			End If

			swOutFile.Close()


		End If

	End Sub

	Public Function ExportMassValues(ByVal strMassValuesFilePath As String) As Boolean

		Dim lstMassInfo As List(Of String)

		Try

			lstMassInfo = GetMassValueList()

			If String.IsNullOrEmpty(strMassValuesFilePath) Then

				Dim lstColWidth = New List(Of Integer) From {10, 12, 0}

				For Each item As String In lstMassInfo
					Dim lstColumns As List(Of String) = item.Split(ControlChars.Tab).ToList()
					Dim strLineOut As String = String.Empty

					For intColIndex As Integer = 0 To lstColumns.Count - 1
						strLineOut &= lstColumns(intColIndex).PadRight(lstColWidth(intColIndex))
					Next
					Console.WriteLine(strLineOut)

				Next
			Else
				Using swOutFile As StreamWriter = New StreamWriter(New FileStream(strMassValuesFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
					For Each item As String In lstMassInfo
						swOutFile.WriteLine(item)
					Next
				End Using
			End If
		Catch ex As Exception
			HandleException("Error in ExportMassValues", ex)
			Return False
		End Try

		Return True

	End Function

	Public Overrides Function GetErrorMessage() As String
		' Returns "" if no error

		Dim strErrorMessage As String

		If MyBase.ErrorCode = eProcessFilesErrorCodes.LocalizedError Or _
		   MyBase.ErrorCode = eProcessFilesErrorCodes.NoError Then
			Select Case mLocalErrorCode
				Case ePeptideFragmentationModellerErrorCodes.NoError
					strErrorMessage = ""

				Case ePeptideFragmentationModellerErrorCodes.InputFileAccessError
					strErrorMessage = "Input file access error"
				Case ePeptideFragmentationModellerErrorCodes.UnspecifiedError
					strErrorMessage = "Unspecified localized error"
				Case Else
					' This shouldn't happen
					strErrorMessage = "Unknown error state"
			End Select
		Else
			strErrorMessage = MyBase.GetBaseClassErrorMessage()
		End If

		Return strErrorMessage

	End Function

	Protected Function GetMassValueList() As List(Of String)

		Dim lstMassInfo As List(Of String)
		lstMassInfo = New List(Of String)

		Dim objMwtWin As New MwtWinDll.MolecularWeightCalculator
		Dim dblMass As Double


		lstMassInfo.Add("Item" & ControlChars.Tab & "Mass" & ControlChars.Tab & "Comment")

		objMwtWin.SetElementMode(MwtWinDll.MWElementAndMassRoutines.emElementModeConstants.emIsotopicMass)

		dblMass = objMwtWin.GetChargeCarrierMass()
		lstMassInfo.Add("Proton" & ControlChars.Tab & dblMass.ToString("0.00000") & ControlChars.Tab & "Charge Carrier")

		lstMassInfo.Add(GetMassValueForElement(objMwtWin, "C", "Element"))
		lstMassInfo.Add(GetMassValueForElement(objMwtWin, "H", "Element"))
		lstMassInfo.Add(GetMassValueForElement(objMwtWin, "N", "Element"))
		lstMassInfo.Add(GetMassValueForElement(objMwtWin, "O", "Element"))
		lstMassInfo.Add(GetMassValueForElement(objMwtWin, "S", "Element"))
		lstMassInfo.Add(GetMassValueForElement(objMwtWin, "P", "Element"))

		lstMassInfo.Add(GetMassValueForElement(objMwtWin, "H", "Peptide N-terminus"))
		lstMassInfo.Add(GetMassValueForElement(objMwtWin, "OH", "Peptide C-terminus"))

		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "A"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "C"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "D"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "E"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "F"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "G"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "H"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "I"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "K"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "L"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "M"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "N"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "P"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "Q"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "R"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "S"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "T"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "U"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "V"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "W"))
		lstMassInfo.Add(GetMassValueForAminoAcid(objMwtWin, "Y"))

		Return lstMassInfo

	End Function

	<CLSCompliant(False)>
	Protected Function GetMassValueForElement(ByVal objMwtWin As MwtWinDll.MolecularWeightCalculator, ByVal strElement As String, ByVal strComment As String) As String
		Dim dblMass As Double
		dblMass = objMwtWin.ComputeMass(strElement)

		Return strElement & ControlChars.Tab & dblMass.ToString("0.00000") & ControlChars.Tab & strComment

	End Function

	<CLSCompliant(False)>
	Protected Function GetMassValueForAminoAcid(ByVal objMwtWin As MwtWinDll.MolecularWeightCalculator, ByVal strAminoAcid As String) As String
		Dim str3LetterAbbrev As String
		Dim dblMass As Double

		str3LetterAbbrev = objMwtWin.GetAminoAcidSymbolConversion(strAminoAcid, True)

		If String.IsNullOrEmpty(str3LetterAbbrev) Then
			Throw New ArgumentOutOfRangeException("Invalid amino acid symbol, " & strAminoAcid)
		End If

		dblMass = objMwtWin.ComputeMass(str3LetterAbbrev)

		Return strAminoAcid & ControlChars.Tab & dblMass.ToString("0.00000") & ControlChars.Tab & "Amino acid, " & str3LetterAbbrev

	End Function

	Private Sub InitializeVariables()
		' File handling options

		mConcatenatedDTA = True
		mConcatenatedDTAFileName = DEFAULT_CONCATENATED_DTA_FILENAME

		mOverwriteExistingFiles = False

		mLocalErrorCode = ePeptideFragmentationModellerErrorCodes.NoError

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

		mCustomModSymbols = New Dictionary(Of Char, Double)
	End Sub


	Public Function LoadParameterFileSettings(ByVal strParameterFilePath As String) As Boolean
		' Returns True if no error; otherwise, returns False
		' If strParameterFilePath is blank, then returns True since this isn't an error

		Const PROCESSING_OPTIONS_SECTION As String = "PeptideFragmentationModeller"

		Dim objSettingsFile As New XmlSettingsFileAccessor

		Try

			If strParameterFilePath Is Nothing OrElse strParameterFilePath.Length = 0 Then
				' No parameter file specified; nothing to load
				Return True
			End If

			If Not File.Exists(strParameterFilePath) Then
				' See if strParameterFilePath points to a file in the same directory as the application
				strParameterFilePath = Path.Combine(Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location), Path.GetFileName(strParameterFilePath))
				If Not File.Exists(strParameterFilePath) Then
					MyBase.SetBaseClassErrorCode(eProcessFilesErrorCodes.ParameterFileNotFound)
					Return False
				End If
			End If

			' There are no parameters to load at the present
			If objSettingsFile.LoadSettings(strParameterFilePath) Then
				If Not objSettingsFile.SectionPresent(PROCESSING_OPTIONS_SECTION) Then
					ShowErrorMessage("The node '<section name=""" & PROCESSING_OPTIONS_SECTION & """> was not found in the parameter file: " & strParameterFilePath)
					MyBase.SetBaseClassErrorCode(eProcessFilesErrorCodes.InvalidParameterFile)
					Return False
				Else

					Me.ConcatenatedDTA = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "ConcatenatedDTA", Me.ConcatenatedDTA)
					Me.ConcatenatedDTAFileName = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "ConcatenatedDTAFileName", Me.ConcatenatedDTAFileName)

					Me.OverwriteExistingFiles = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "OverwriteExistingFiles", Me.OverwriteExistingFiles)

					Me.ShowAIons = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "ShowAIons", Me.ShowAIons)
					Me.ShowBIons = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "ShowBIons", Me.ShowBIons)
					Me.ShowCIons = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "ShowCIons", Me.ShowCIons)
					Me.ShowYIons = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "ShowYIons", Me.ShowYIons)
					Me.ShowZIons = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "ShowZIons", Me.ShowZIons)

					Me.NeutralLossAmmonia = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "NeutralLossAmmonia", Me.NeutralLossAmmonia)
					Me.NeutralLossPhosphate = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "NeutralLossPhosphate", Me.NeutralLossPhosphate)
					Me.NeutralLossWater = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "NeutralLossWater", Me.NeutralLossWater)

					Me.IncludeShoulderIons = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "IncludeShoulderIons", Me.IncludeShoulderIons)

					Me.IncludeDoublyChargedIons = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "IncludeDoublyChargedIons", Me.IncludeDoublyChargedIons)
					Me.DoubleChargeMZThreshold = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "DoubleChargeMZThreshold", Me.DoubleChargeMZThreshold)

					Me.IncludeTriplyChargedIons = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "IncludeTriplyChargedIons", Me.IncludeTriplyChargedIons)
					Me.TripleChargeMZThreshold = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "TripleChargeMZThreshold", Me.TripleChargeMZThreshold)

					Me.LabelIons = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "LabelIons", Me.LabelIons)
					Me.LabelIonsVerbose = objSettingsFile.GetParam(PROCESSING_OPTIONS_SECTION, "LabelIonsVerbose", Me.LabelIonsVerbose)

				End If

			End If

		Catch ex As Exception
			HandleException("Error in LoadParameterFileSettings", ex)
			Return False
		End Try

		Return True

	End Function

	' Main processing function (utilizes EvaluateMsMsSpectrum)
	Public Overloads Overrides Function ProcessFile(ByVal strInputFilePath As String, ByVal strOutputFolderPath As String, ByVal strParameterFilePath As String, ByVal blnResetErrorCode As Boolean) As Boolean
		' Returns True if success, False if failure

		Dim ioFile As FileInfo

		Dim strInputFilePathFull As String
		Dim strStatusMessage As String

		Dim blnSuccess As Boolean

		If blnResetErrorCode Then
			SetLocalErrorCode(ePeptideFragmentationModellerErrorCodes.NoError)
		End If

		If Not LoadParameterFileSettings(strParameterFilePath) Then
			strStatusMessage = "Parameter file load error: " & strParameterFilePath
			ShowErrorMessage(strStatusMessage)

			If MyBase.ErrorCode = eProcessFilesErrorCodes.NoError Then
				MyBase.SetBaseClassErrorCode(eProcessFilesErrorCodes.InvalidParameterFile)
			End If
			Return False
		End If

		Try
			If strInputFilePath Is Nothing OrElse strInputFilePath.Length = 0 Then
				Console.WriteLine("Input file name is empty")
				MyBase.SetBaseClassErrorCode(eProcessFilesErrorCodes.InvalidInputFilePath)
			Else

				'Console.WriteLine()
				'Console.WriteLine("Parsing " & Path.GetFileName(strInputFilePath))

				If Not CleanupFilePaths(strInputFilePath, strOutputFolderPath) Then
					MyBase.SetBaseClassErrorCode(eProcessFilesErrorCodes.FilePathError)
				Else
					Try

						' Obtain the full path to the input file
						ioFile = New FileInfo(strInputFilePath)
						Console.WriteLine("Processing file: " & strInputFilePath)
						strInputFilePathFull = ioFile.FullName

						blnSuccess = ProcessPeptideSequencesFile(strInputFilePathFull, strOutputFolderPath)

						If Not blnSuccess Then
							SetLocalErrorCode(ePeptideFragmentationModellerErrorCodes.InputFileAccessError, True)
						End If

					Catch ex As Exception
						HandleException("Error calling ProcessDtaFile or ProcessDtaTxtFile", ex)
					End Try
				End If
			End If
		Catch ex As Exception
			HandleException("Error in ProcessFile", ex)
		End Try

		Return blnSuccess

	End Function

	Private Function ProcessPeptideSequencesFile(ByVal strInputFilePath As String, ByVal strOutputFolderPath As String) As Boolean


		Dim srInFile As StreamReader
		Dim objMwtWin As New MwtWinDll.MolecularWeightCalculator

		Dim strLineIn As String

		Dim udtFragSpectrumOptions As MwtWinDll.MWPeptideClass.udtFragmentationSpectrumOptionsType
		Dim udtFragSpectrum() As MwtWinDll.MWPeptideClass.udtFragmentationSpectrumDataType = Nothing

		Dim blnSuccess As Boolean

		Dim dblMass As Double
		Dim dblMH As Double

		Dim dblChargeCarrierMass As Double
		Dim strDtaFileName As String
		Dim intMatchCount As Integer

		blnSuccess = True

		Try

			' Open strInputFilePath and read each sequence
			' Fragment each with  objMwtWin
			' Create a .dta file using the data

			srInFile = New StreamReader(strInputFilePath)

			dblChargeCarrierMass = objMwtWin.GetChargeCarrierMass()

			' Switch to monoisotopic masses
			objMwtWin.SetElementMode(MwtWinDll.MWElementAndMassRoutines.emElementModeConstants.emIsotopicMass)

			udtFragSpectrumOptions = objMwtWin.Peptide.GetFragmentationSpectrumOptions()

			With udtFragSpectrumOptions

				.DoubleChargeIonsShow = mIncludeDoublyChargedIons
				.DoubleChargeIonsThreshold = mDoubleChargeMZThreshold

				.TripleChargeIonsShow = mIncludeTriplyChargedIons
				.TripleChargeIonsThreshold = mTripleChargeMZThreshold


				With .IntensityOptions
					.IonType(MwtWinDll.MWPeptideClass.itIonTypeConstants.itAIon) = MwtWinDll.MWPeptideClass.DEFAULT_A_ION_INTENSITY

					.IonType(MwtWinDll.MWPeptideClass.itIonTypeConstants.itBIon) = MwtWinDll.MWPeptideClass.DEFAULT_BYCZ_ION_INTENSITY
					.IonType(MwtWinDll.MWPeptideClass.itIonTypeConstants.itYIon) = MwtWinDll.MWPeptideClass.DEFAULT_BYCZ_ION_INTENSITY

					.IonType(MwtWinDll.MWPeptideClass.itIonTypeConstants.itCIon) = MwtWinDll.MWPeptideClass.DEFAULT_BYCZ_ION_INTENSITY
					.IonType(MwtWinDll.MWPeptideClass.itIonTypeConstants.itZIon) = MwtWinDll.MWPeptideClass.DEFAULT_BYCZ_ION_INTENSITY

					If mIncludeShoulderIons Then
						.BYIonShoulder = MwtWinDll.MWPeptideClass.DEFAULT_B_Y_ION_SHOULDER_INTENSITY
					Else
						.BYIonShoulder = 0
					End If

					.NeutralLoss = MwtWinDll.MWPeptideClass.DEFAULT_NEUTRAL_LOSS_ION_INTENSITY
				End With

				ReDim .IonTypeOptions(4)

				With .IonTypeOptions(MwtWinDll.MWPeptideClass.itIonTypeConstants.itAIon)
					.ShowIon = mShowAIons
					.NeutralLossWater = mNeutralLossWater
					.NeutralLossAmmonia = mNeutralLossAmmonia
					.NeutralLossPhosphate = mNeutralLossPhosphate
				End With

				With .IonTypeOptions(MwtWinDll.MWPeptideClass.itIonTypeConstants.itBIon)
					.ShowIon = mShowBIons
					.NeutralLossWater = mNeutralLossWater
					.NeutralLossAmmonia = mNeutralLossAmmonia
					.NeutralLossPhosphate = mNeutralLossPhosphate
				End With

				With .IonTypeOptions(MwtWinDll.MWPeptideClass.itIonTypeConstants.itYIon)
					.ShowIon = mShowYIons
					.NeutralLossWater = mNeutralLossWater
					.NeutralLossAmmonia = mNeutralLossAmmonia
					.NeutralLossPhosphate = mNeutralLossPhosphate
				End With

				With .IonTypeOptions(MwtWinDll.MWPeptideClass.itIonTypeConstants.itCIon)
					.ShowIon = mShowCIons
					.NeutralLossWater = mNeutralLossWater
					.NeutralLossAmmonia = mNeutralLossAmmonia
					.NeutralLossPhosphate = mNeutralLossPhosphate
				End With

				With .IonTypeOptions(MwtWinDll.MWPeptideClass.itIonTypeConstants.itZIon)
					.ShowIon = mShowZIons
					.NeutralLossWater = mNeutralLossWater
					.NeutralLossAmmonia = mNeutralLossAmmonia
					.NeutralLossPhosphate = mNeutralLossPhosphate
				End With

			End With

			objMwtWin.Peptide.SetFragmentationSpectrumOptions(udtFragSpectrumOptions)

			SetModificationsInDll(objMwtWin, mCustomModSymbols)

			If mConcatenatedDTA Then
				If String.IsNullOrEmpty(mConcatenatedDTAFileName) Then
					mConcatenatedDTAFileName = DEFAULT_CONCATENATED_DTA_FILENAME
				End If


				Try
					If File.Exists(mConcatenatedDTAFileName) Then
						' Existing concatenated DTA file found
						ShowMessage("Existing CDTA file found: " & Path.GetFileName(mConcatenatedDTAFileName))

						If mOverwriteExistingFiles Then
							' Clear it now
							ShowMessage(" - Overwriting the existing file")
							File.Delete(mConcatenatedDTAFileName)
						Else
							ShowMessage(" - Appending to the existing file ( use /Overwrite to overwrite)")
						End If

					End If
				Catch ex As Exception
					HandleException("Error clearing the concatenated DTA file: " & mConcatenatedDTAFileName, ex)
				End Try

			End If

			Do While srInFile.Peek() >= 0
				strLineIn = srInFile.ReadLine()
				If Not strLineIn Is Nothing AndAlso strLineIn.Length > 0 Then
					strLineIn = strLineIn.Trim()

					objMwtWin.Peptide.SetSequence(strLineIn, MwtWinDll.MWPeptideClass.ntgNTerminusGroupConstants.ntgHydrogen, MwtWinDll.MWPeptideClass.ctgCTerminusGroupConstants.ctgHydroxyl, False, True)

					dblMass = objMwtWin.Peptide.GetPeptideMass()
					objMwtWin.Peptide.GetFragmentationMasses(udtFragSpectrum)

					dblMH = dblMass + dblChargeCarrierMass
					strDtaFileName = strOutputFolderPath & "\" & CStr(strLineIn) & "." & CStr(dblMH)

					If dblMass < 1000 Then
						' Write out a 1+ .dta file

						CreateDtaFile(strDtaFileName, udtFragSpectrum, dblMH, 1)

					ElseIf dblMass >= 1000 And dblMass < 2000 Then
						' Write out a 1+ and a 2+ .dta file

						CreateDtaFile(strDtaFileName, udtFragSpectrum, dblMH, 1)
						CreateDtaFile(strDtaFileName, udtFragSpectrum, dblMH, 2)

					Else
						' dblMass id > 2000
						' Write out a 2+ and a 3+ .dta file

						CreateDtaFile(strDtaFileName, udtFragSpectrum, dblMH, 2)
						CreateDtaFile(strDtaFileName, udtFragSpectrum, dblMH, 3)

					End If
				End If
				intMatchCount += 1

				If intMatchCount Mod 100 = 0 Then Console.Write(".")
			Loop

			Console.WriteLine(" ")
			srInFile.Close()



		Catch ex As Exception
			SetLocalErrorCode(ePeptideFragmentationModellerErrorCodes.InputFileAccessError)
			blnSuccess = False
		End Try

		Return blnSuccess

	End Function

	Private Sub SetLocalErrorCode(ByVal eNewErrorCode As ePeptideFragmentationModellerErrorCodes)
		SetLocalErrorCode(eNewErrorCode, False)
	End Sub

	Private Sub SetLocalErrorCode(ByVal eNewErrorCode As ePeptideFragmentationModellerErrorCodes, ByVal blnLeaveExistingErrorCodeUnchanged As Boolean)

		If blnLeaveExistingErrorCodeUnchanged AndAlso mLocalErrorCode <> ePeptideFragmentationModellerErrorCodes.NoError Then
			' An error code is already defined; do not change it
		Else
			mLocalErrorCode = eNewErrorCode

			If eNewErrorCode = ePeptideFragmentationModellerErrorCodes.NoError Then
				If MyBase.ErrorCode = eProcessFilesErrorCodes.LocalizedError Then
					MyBase.SetBaseClassErrorCode(eProcessFilesErrorCodes.NoError)
				End If
			Else
				MyBase.SetBaseClassErrorCode(eProcessFilesErrorCodes.LocalizedError)
			End If
		End If

	End Sub

	Private Sub SetModificationsInDll(ByRef objMwtWin As MwtWinDll.MolecularWeightCalculator, _
									  ByRef objModList As Dictionary(Of Char, Double))

		Dim objEnum As Dictionary(Of Char, Double).Enumerator
		Dim blnIndicatesPhosphorylation As Boolean

		If Not objModList Is Nothing Then
			objEnum = objModList.GetEnumerator()
			Do While objEnum.MoveNext
				If objEnum.Current.Key = "*"c AndAlso Math.Abs(79.9663 - objEnum.Current.Value) <= 0.1 Then
					blnIndicatesPhosphorylation = True
				Else
					blnIndicatesPhosphorylation = False
				End If

				objMwtWin.Peptide.SetModificationSymbol(objEnum.Current.Key, objEnum.Current.Value, blnIndicatesPhosphorylation, "")

				LogMessage("Defined custom modification: " & objEnum.Current.Key & "=" & objEnum.Current.Value, eMessageTypeConstants.Normal)
			Loop
		End If

	End Sub

End Class
