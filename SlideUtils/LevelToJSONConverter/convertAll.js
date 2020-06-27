const fs = require('fs');
const { spawn } = require('child_process');

console.log('Converting ASCII char levels to JSON levels');

let packs = [];
let packDirs = fs.readdirSync(`${__dirname}\\..\\..\\SlideAssets\\CharLevels\\`)
	.filter(d => fs.statSync(`${__dirname}\\..\\..\\SlideAssets\\CharLevels\\${d}`).isDirectory());

packDirs.forEach(packDir => {
	packs.push({
		PackID: packDir,
		PackName: packDir.replace('_', ' '),
		Levels: fs.readdirSync(`${__dirname}\\..\\..\\SlideAssets\\CharLevels\\${packDir}`)
			.filter(f => fs.statSync(`${__dirname}\\..\\..\\SlideAssets\\CharLevels\\${packDir}\\${f}`).isFile())
			.map(file => {
				let fileName = file.replace(new RegExp('(\\.txt|\\.json)'), '')
				let displayName = fileName.substring(file.indexOf('_') + 1).replace('_', ' - ');
				while (displayName.startsWith(0))
					displayName = displayName.slice(1);
				return {
					ID: `${packDir}\\${fileName}`,
					DisplayName: displayName,
				}
			})
	});
});

packs.forEach(pack => {
	const proc = spawn('node', [`${__dirname}\\convert.js`, '-l', pack.Levels.map(l => l.ID).join(',')]);
	proc.stdout.on('data', (data) => {
		//console.log(data.toString());
	});
	proc.stderr.on('data', (data) => {
		console.log(data.toString());
	});
	proc.on('exit', data => {
		console.log('Finished converting ASCII char levels to JSON levels');
	})

	let packJSON = JSON.stringify(pack, null, '\t');
	fs.writeFileSync(`${__dirname}\\..\\..\\SlideCore\\SlideCore\\Content\\LevelPack\\${pack.PackID}.json`, packJSON)
});