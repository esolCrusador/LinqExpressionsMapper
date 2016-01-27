param($installPath, $toolsPath, $package, $project)
$currentVersion = 1;
$versionVariableName = 'Previous '+'LinqExpressionsMapper'+' Script Version For '+$project.ProjectName;

Set-Variable -Name $versionVariableName -Value $currentVersion -Scope Global;