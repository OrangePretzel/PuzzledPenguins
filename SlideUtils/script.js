// // ========================================
// // Generate a new Level Pack
// // ========================================
// let levelPackName = "NewLevelPack";
// let startRow = 6;

// const fs = require('fs');
// fs.mkdirSync(`${__dirname}\\..\\SlideAssets\\CharLevels\\${levelPackName}`);
// for (let i = 0; i < 5; i++)
// 	for (let j = 0; j < 5; j++)
// 		fs.writeFileSync(`${__dirname}\\..\\SlideAssets\\CharLevels\\${levelPackName}\\Level_${startRow + i}_${j + 1}.txt`, '......\n..@..X\n...#..\n......\n..X...\n......');
// // ========================================

// // ========================================
// // Rename Level Files
// // ========================================
// let levelPackName = "Warehouse_1";
// let startRow = 6;

// const fs = require('fs');
// fs.readdirSync(`${__dirname}\\..\\SlideAssets\\CharLevels\\${levelPackName}`)
// 	.filter(f => fs.statSync(`${__dirname}\\..\\SlideAssets\\CharLevels\\${levelPackName}\\${f}`).isFile())
// 	.filter(f => f.slice(-8).startsWith('_'))
// 	.forEach(f => fs.renameSync(
// 		`${__dirname}\\..\\SlideAssets\\CharLevels\\${levelPackName}\\${f}`,
// 		`${__dirname}\\..\\SlideAssets\\CharLevels\\${levelPackName}\\${f.slice(0, -8)}_0${f.slice(-7)}`));
// // ========================================