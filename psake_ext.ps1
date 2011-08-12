
function Get-File-Exists-On-Path
{
	param(
		[string]$file
	)
	$results = ($Env:Path).Split(";") | Get-ChildItem -filter $file -erroraction silentlycontinue
	$found = ($results -ne $null)
	return $found
}

function Get-Batchfile {
param(
	[string]$file = $(throw "file is a required parameter.")
)
	$cmd = "`"$file`" & set"
  cmd /c $cmd | Foreach-Object {
      $p, $v = $_.split('=')
      Set-Item -path env:$p -value $v
  }
}

function VsVars32 {
param(
	[string]$version = "10.0"
)
	if([intptr]::size -eq 8) {
		$key = "HKLM:SOFTWARE\Wow6432Node\Microsoft\VisualStudio\" + $version
	} else {
		$key = "HKLM:SOFTWARE\Microsoft\VisualStudio\" + $version
	}

  $VsKey = get-ItemProperty $key
  $VsInstallPath = [System.IO.Path]::GetDirectoryName($VsKey.InstallDir)
  $VsToolsDir = [System.IO.Path]::GetDirectoryName($VsInstallPath)
  $VsToolsDir = [System.IO.Path]::Combine($VsToolsDir, "Tools")
  $BatchFile = [System.IO.Path]::Combine($VsToolsDir, "vsvars32.bat")
  Get-Batchfile $BatchFile
  [System.Console]::Title = "Visual Studio $version Windows Powershell"
}

function Get-Git-Commit
{
	if ((Get-File-Exists-On-Path "git.exe")){
		$gitLog = git log --oneline -1
		return $gitLog.Split(' ')[0]
	}
	else {
		return "0000000"
	}
}


function Delete-Sample-Data-For-Release
{
  param([string]$sample_dir)
  rd "$sample_dir\bin" -force -recurse -ErrorAction SilentlyContinue
  rd "$sample_dir\obj" -force -recurse -ErrorAction SilentlyContinue
  
  rd "$sample_dir\Servers\Shard1\Data" -force -recurse -ErrorAction SilentlyContinue
  rd "$sample_dir\Servers\Shard2\Data" -force -recurse -ErrorAction SilentlyContinue
  
  rd "$sample_dir\Servers\Shard1\Plugins" -force -recurse -ErrorAction SilentlyContinue
  rd "$sample_dir\Servers\Shard2\Plugins" -force -recurse -ErrorAction SilentlyContinue
  
  
  del "$sample_dir\Servers\Shard1\RavenDB.exe" -force -recurse -ErrorAction SilentlyContinue
  del "$sample_dir\Servers\Shard2\RavenDB.exe" -force -recurse -ErrorAction SilentlyContinue
}


function Generate-Assembly-Info
{
param(
	[string]$clsCompliant = "true",
	[string]$title, 
	[string]$description, 
	[string]$company, 
	[string]$product, 
	[string]$copyright, 
	[string]$version,
	[string]$fileVersion,
	[string]$file = $(throw "file is a required parameter."),
	[string]$configuration = ""
)
  if($env:buildlabel -eq 9999 -and (Test-Path $file))
	{
		Write-Host "Suppressing Generation of Assembly Info"
    return
	}

	$commit = Get-Git-Commit
   
  $asmInfo = "using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if !SILVERLIGHT
//[assembly: SuppressIldasmAttribute()]
[assembly: CLSCompliantAttribute($clsCompliant )]
#endif
[assembly: ComVisibleAttribute(false)]
[assembly: AssemblyTitleAttribute(""$title"")]
[assembly: AssemblyDescriptionAttribute(""$description"")]
[assembly: AssemblyCompanyAttribute(""$company"")]
[assembly: AssemblyProductAttribute(""$product"")]
[assembly: AssemblyCopyrightAttribute(""$copyright"")]
[assembly: AssemblyVersionAttribute(""$version"")]
[assembly: AssemblyInformationalVersionAttribute(""$version / $commit"")]
[assembly: AssemblyFileVersionAttribute(""$fileVersion"")]
[assembly: AssemblyDelaySignAttribute(false)]
[assembly: AssemblyTrademark("""")]
[assembly: AssemblyCulture("""")]
[assembly: AssemblyConfiguration(""$config"")]
"

	$dir = [System.IO.Path]::GetDirectoryName($file)
	if ([System.IO.Directory]::Exists($dir) -eq $false)
	{
		Write-Host "Creating directory $dir"
		[System.IO.Directory]::CreateDirectory($dir)
	}
	Write-Host "Generating assembly info file: $file"
	Write-Output $asmInfo > $file
	#Write-Host $file
	#Write-Host $asmInfo
}

function Build-Solution {
param(
	[string]$solutionFile = $(throw "solution is a required parameter."),
	[string]$outputDir = $(throw "output directory is a required parameter."),
	[string]$target = "Rebuild",
	[string]$verbosity = "minimal"
)
	$v4_net_version = (ls "$env:windir\Microsoft.NET\Framework\v4.0*").Name
	#exec { &"c:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" ""$slnFile"" /v:m /t:Rebuild /p:OutDir=""$$outputDir\"" }
	Write-Host "/p:OutDir=""$outputDir\"""
	exec { "C:\Windows\Microsoft.NET\Framework\$v4_net_version\MSBuild.exe `"$slnFile`" /v:$verbosity /t:$target /p:OutDir=`"$outputDir\`"" }
}

function Build-Project {
param(
	[Parameter(Position=0,Mandatory=1)]
	[string]$projectFile,
	[Parameter(Position=1,Mandatory=1)]
	[string]$outputDir,
	[Parameter(Position=2,Mandatory=0)]
	[string]$configuraiton = "Release",
	[Parameter(Position=3,Mandatory=0)]
	[string]$target = "Rebuild",
	[Parameter(Position=4,Mandatory=0)]
	[string]$verbosity = "minimal"
)
	#$v4_net_version = (ls "$env:windir\Microsoft.NET\Framework\v4.0*").Name
	#$projectDir = [System.IO.Path]::GetDirectoryName($projectFile)
	#$projectName = [System.IO.Path]::GetFileName($projectDir)
	
 	#$old = pwd
  #cd $projectDir

	Write-Host "ProjectFile: $projectFile"
	Write-Host "outputDir: $outputDir"
	Write-Host "configuraiton: $configuraiton"
	Write-Host "target: $target"
	Write-Host "verbosity: $verbosity"
	
	#exec { &"MSBuild.exe" "'"$projectFile'" /v:$verbosity" }

	#Write-Host "MSBuild.exe `"$projectFile`" /p:OutDir=`"$outputDir\`" /t:$target /v:$verbosity /p:Configuration=$configuration"
	#exec { &"MSBuild.exe" `"$projectFile`" /p:OutDir=`"$outputDir\`" /t:$target /v:$verbosity /p:Configuration=$configuration }
    # exec { &"C:\Windows\Microsoft.NET\Framework\$v4_net_version\MSBuild.exe" /t:$target /v:$verbosity /p:OutDir=`"$outputDir\`" `"$projectName`" }

  #cd $old
}
