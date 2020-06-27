const fs = require('fs');
const { spawn } = require('child_process');

const args = require('yargs')
	.option('levelIDs', {
		alias: 'l',
		describe: 'The ID of the level',
		demandOption: true,
		type: 'string'
	})
	.argv;

let levelIDs = args.levelIDs.split(',');

levelIDs.forEach(levelID => {
	let outputDir = `${__dirname}/../../SlideCore/SlideCore/Content/Level/`;
	if (!fs.existsSync(outputDir)) {
		fs.mkdirSync(outputDir);
	}

	let jsonInputFile = `${__dirname}/../../SlideAssets/CharLevels/${levelID}.json`;
	if (fs.existsSync(jsonInputFile)) {
		var content = fs.readFileSync(jsonInputFile);
		fs.writeFileSync(`${outputDir}/${levelID}.json`, content, { flag: 'w' });
		console.log(`Successfully copied ${jsonInputFile} to ${outputDir}${levelID}.json`);
		return;
	}

	let inputFile = `${__dirname}/../../SlideAssets/CharLevels/${levelID}.txt`;

	const convertProc = spawn('node', [`${__dirname}/levelToJSON.js`, '-i', inputFile, '-o', outputDir, '-l', levelID]);
	convertProc.stdout.on('data', (data) => {
		console.log(`stdout: ${data}`);
	});
	convertProc.stderr.on('data', (data) => {
		console.log(`stderr: ${data}`);
	});
	convertProc.on('exit', (code) => {
		if (code === 0) console.log(`Successfully converted ${inputFile} to ${outputDir}${levelID}.json`);
		else console.error(`Conversion of ${inputFile} to ${outputDir}${levelID}.json failed!`);
	});
});