Get-ChildItem *.svg | % { 
	magick convert `
		-background none `
		-density 1200 `
		-resize 64 `
		$_.Name `
		"$($_.BaseName).png"
}
