// Sample Command:
// node .\levelToJSON.js -i ..\..\temp\charLevel.txt -o ..\..\SlideUnity\Assets\Resources\Levels -l DefaultLevels/Level_1_3

const fs = require('fs');
const path = require('path');

const args = require('yargs')
	.option('inputFile', {
		alias: 'i',
		describe: 'The file containing the input',
		demandOption: true,
		type: 'string'
	})
	.option('levelID', {
		alias: 'l',
		describe: 'The ID of the level',
		demandOption: true,
		type: 'string'
	})
	.option('outputDir', {
		alias: 'o',
		describe: 'The directory in which to put the output',
		demandOption: true,
		type: 'string'
	})
	.argv;

let inputFile = args.inputFile;
let outputDir = args.outputDir;
let levelID = args.levelID;

let input = fs.readFileSync(args.inputFile).toString();
input = input.replace(new RegExp('[ ]*', 'g'), '');
let inputLines = input.split(new RegExp('\\r?\\n'));

let targetMoves = 0;
let lineOffset = 0;
if (inputLines[0].match('^[0-9]')) {
	lineOffset += 1;
	targetMoves = parseInt(inputLines[0]);
}

let levelHeight = inputLines.length - lineOffset;
if (levelHeight < 1)
	throw `Input file [${inputFile}] does not contain valid input`;
let levelWidth = inputLines[0 + lineOffset]
	.replace(new RegExp('(\\^\\d)', 'g'), '$')
	.replace(new RegExp('(P{\\d+,\\d+})', 'g'), '$').length;

let entities = [];
for (let y = 0; y < levelHeight; ++y) {
	let line = inputLines[y + lineOffset];
	let char = 0;
	for (let x = 0; x < levelWidth; ++x) {
		switch (line[char++]) {
			case '@': // Player
				entities.push({
					Type: "Player",
					X: x,
					Y: y
				});
				break;
			case '#': // FinishFlag
				entities.push({
					Type: "FinishFlag",
					X: x,
					Y: y
				});
				break;
			case 'X': // Wall
				entities.push({
					Type: "Wall",
					X: x,
					Y: y
				});
				break;
			case '^': // RedirectTile
				entities.push({
					Type: "RedirectTile",
					X: x,
					Y: y,
					RedirectDir: parseInt(line[char++])
				});
				break;
			case 'O': // HaltTile
				entities.push({
					Type: "HaltTile",
					X: x,
					Y: y
				});
				break;
			case 'P': // Portal
				let delimiter;
				char++; // {

				// Portal Target X
				delimiter = line.indexOf(",", char);
				let pX = parseInt(line.substring(char, delimiter));
				char = delimiter + 1;

				// Portal Target Y
				delimiter = line.indexOf("}", char);
				let pY = parseInt(line.substring(char, delimiter));
				char = delimiter + 1;

				entities.push({
					Type: "Portal",
					X: x,
					Y: y,
					PortalTarget: {
						X: pX,
						Y: pY
					}
				})
				break;
			case 'C': // SlidingCrate
				entities.push({
					Type: "SlidingCrate",
					X: x,
					Y: y
				});
				break;
			case 'U': // Pit
				entities.push({
					Type: "Pit",
					X: x,
					Y: y
				});
				break;
			case 'J': // JumpTile
				entities.push({
					Type: "JumpTile",
					X: x,
					Y: y
				});
				break;
			case 'D': // WireDoor
				entities.push({
					Type: "WireDoor",
					X: x,
					Y: y
				});
				break;
			case 'B': // WireButton
				entities.push({
					Type: "WireButton",
					X: x,
					Y: y,
					Mode: "PressOnly",
					Wire: [
						{
							X: 0,
							Y: 0,
							Invert: false
						}
					]
				});
				break;
			case 'S': // Sushi
				entities.push({
					Type: "Sushi",
					X: x,
					Y: y
				});
				break;
			case '.':
				break; // Ignore
			default:
				throw `Unknown Symbol ${line[char - 1]} found in file at position ${char - 1}`;
		}
	}
}

let isWallAt = (x, y) => {
	let e = entities.find(entity => entity.X === x && entity.Y === y);
	return e != undefined && e.Type == 'Wall';
}

entities.forEach(entity => {
	if (entity.Type === 'Wall') {
		let walls = 0
			+ (isWallAt(entity.X + 1, entity.Y + 0) ? 1 << 0 : 0)
			+ (isWallAt(entity.X + 1, entity.Y - 1) ? 1 << 1 : 0)
			+ (isWallAt(entity.X + 0, entity.Y - 1) ? 1 << 2 : 0)
			+ (isWallAt(entity.X - 1, entity.Y - 1) ? 1 << 3 : 0)
			+ (isWallAt(entity.X - 1, entity.Y + 0) ? 1 << 4 : 0)
			+ (isWallAt(entity.X - 1, entity.Y + 1) ? 1 << 5 : 0)
			+ (isWallAt(entity.X + 0, entity.Y + 1) ? 1 << 6 : 0)
			+ (isWallAt(entity.X + 1, entity.Y + 1) ? 1 << 7 : 0)
		entity.AdjacentWalls = walls;
	}
});

const levelFilePath = path.join(outputDir, levelID + '.json');
const levelJSON = JSON.stringify({
	Parser: "STD1",
	TargetMoves: targetMoves,
	Width: levelWidth,
	Height: levelHeight,
	Entities: entities
}, null, '\t');

try {
	fs.mkdirSync(path.dirname(levelFilePath));
} catch (err) {
	if (err.code == 'EEXIST') { } // Ignore
	else throw err;
}
fs.writeFileSync(levelFilePath, levelJSON);
