[string[]] $replaceRegexes = "Mapper.Map\<(?<TSource>[\w+_\.]+), ?(?<TDest>[\w+_\.]+)\>\((?<arg1>[\w+_\.\d\[\]]+)\)", 
							 "Mapper.Map\<(?<TSource>[\w+_\.]+), ?(?<TDest>[\w+_\.]+)\>\((?<arg1>[\w+_\.\d\[\]]+), ? (?<arg2>[\w+_\.\d\[\]]+)\)",
							 "Mapper.Map\<(?<TMapper>[\w+_\.]+), ?(?<TSource>[\w+_\.]+), ?(?<TDest>[\w+_\.]+)\>\((?<arg1>[\w\d\[\]]+)\)",
							 "Mapper.Map\<(?<TMapper>[\w+_\.]+), ?(?<TSource>[\w+_\.]+), ?(?<TDest>[\w+_\.]+)\>\((?<arg1>[\w\d\[\]]+), ? (?<arg2>[\w+_\.\d\[\]]+)\)",
							 "ResolveSelect\<(?<TSource>[\w+_\.]+), ?(?<TDest>[\w+_\.]+)\>\( ?\)"

[string[]] $replaces = 'Mapper.From<${TSource}>(${arg1}).To<${TDest}>().Map()',
					   'Mapper.From<${TSource}>(${arg1}).To<${TDest}>(${arg2}).Map()',
					   'Mapper.From<${TSource}>(${arg1}).To<${TDest}>().Using<${TMapper}>().Map()',
					   'Mapper.From<${TSource}>(${arg1}).To<${TDest}>(${arg2}).Using<${TMapper}>().Map()',
					   'Project<${TSource}>().To<${TDest}>().Queryable'

$projectFiles;

function GetFiles($projectItems)
{
	[string[]] $results = @();
	foreach($projectItem in $projectItems)
	{
		# Physical File https://msdn.microsoft.com/en-us/library/z4bcch80(v=vs.80).aspx
		if($projectItem.Kind -eq "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}")
		{
			$fileName=$projectItem.FileNames(0);
			if(-not [System.String]::IsNullOrEmpty($fileName))
			{
				$extension = [System.IO.Path]::GetExtension($fileName);

				if($extension -eq ".cs" -or $extension -eq ".vb" -or $extension -eq ".cshtml" -or $extension -eq ".vbhtml" -or $extension -eq ".xaml")
				{
					$results += $fileName;
				}
			}
		}
		else{
			$files = GetFiles($projectItem.ProjectItems);

			foreach($fileName in $files)
			{
				$results += $fileName;
			}
		}
	}

	return $results;
}

$projectFiles = GetFiles($project.ProjectItems);

[string[]] $files2Rename = @();

foreach($projectFile in $projectFiles)
{
	$fileText = [System.IO.File]::ReadAllText($projectFile);

	foreach($replaceRegex in $replaceRegexes)
	{
		if($fileText -match $replaceRegex)
		{
			$files2Rename += $projectFile;
			break;
		}
	}
}

foreach($file2Rename in $files2Rename)
{
	$dte.ExecuteCommand("File.OpenFile", $file2Rename);
	$projectItem = $dte.Solution.FindProjectItem($file2Rename);

	for($i = 0; $i -le $replaceRegexes.length-1; $i++)
	{
		$projectItem.Document.ReplaceText($replaceRegexes[$i], $replaces[$i], [int][EnvDTE.vsFindOptions]::vsFindOptionsRegularExpression);
	}
}