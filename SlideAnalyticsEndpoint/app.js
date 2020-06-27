const fs = require('fs');

const express = require('express');
const http = require('http');

const port = process.env.PORT || 80;
const app = express();

const bodyParser = require('body-parser');
app.use(bodyParser.json()); // support json encoded bodies
app.use(bodyParser.urlencoded({ extended: true })); // support encoded bodies

const { google } = require('googleapis');
const drive = google.drive({ version: 'v3' });
const jwtClient = new google.auth.JWT(
	process.env.google_client_email,
	null,
	process.env.google_private_key.replace(/\\n/g, '\n'),
	['https://www.googleapis.com/auth/drive'],
	null
);

let analyticsBuffer = [];

authorizeGoogleDriveAsync()
	.then(() => startServer())
	.catch(err => console.log(err));

function authorizeGoogleDriveAsync() {
	return new Promise((resolve, reject) => {
		jwtClient.authorize((authErr) => {
			if (authErr) reject(authErr);
			else resolve();
		});
	});
}

function startServer() {
	app.get('/ping', (req, res) => {
		console.log(`App pinged at ${new Date().toUTCString()}`);
		res.send();
	});

	app.post('/flush', (req, res) => {
		console.log(`Flushing analytics as per POST request`);
		flushAnalytics()
			.then(() => res.sendStatus(200))
			.catch(err => {
				console.log(`Could not flush analtics due to error: ${err}`);
				res.sendStatus(500);
			});
	});

	app.post('/solve', (req, res) => {
		// Get input
		let playerID = req.body.playerID;
		let levelID = req.body.levelID;
		let solution = req.body.solution;

		// Validate input
		if (isNullOrWhitespace(playerID)) {
			res.status(400);
			res.send('Invalid playerID provided');
			return;
		}

		if (isNullOrWhitespace(levelID)) {
			res.status(400);
			res.send('Invalid levelID provided');
			return;
		}

		const validSolutionRegex = /^[RLUDN]+$/i;
		if (isNullOrWhitespace(solution) || !validSolutionRegex.test(solution)) {
			res.status(400);
			res.send('Invalid solution provided');
			return;
		}

		// Cleanup input
		playerID = playerID.trim();
		levelID = levelID.trim();
		solution = solution.trim();

		console.log(`Player ${playerID} solved level ${levelID} with this solution: ${solution}`);
		analyticsBuffer.push({
			playerID: playerID,
			levelID: levelID,
			solution: solution
		});

		// Success
		res.send();
	});

	app.on('close_server', () => {
		console.log("Server closing. Flushing analytics before exiting")
		flushAnalytics();
		console.log("Goodbye!")
	});

	const server = app.listen(port, () => {
		console.log(`App listening on ${server.address().address}:${server.address().port}`);

		// Flush analytics every 25 minutes, as the sleep time for Heroku kicks in at 30 minutes
		// This guarentees no analytics data is lost from sleep timeout
		setInterval(() => {
			flushAnalytics()
				.catch(err => console.log(`Could not flush analtics due to error: ${err}`));
		}, 25 * 60 * 1000);
	});
}

function getFolderByName(folderName) {
	return new Promise((resolve, reject) => {
		drive.files.list({ auth: jwtClient },
			(err, resp) => {
				if (err) {
					reject(err);
					return;
				}
				let file = resp.data.files.find(f => f.name === folderName);
				if (file != undefined) {
					resolve(file);
					return;
				}
				resolve();
			})
	});
}

function flushAnalytics() {
	if (analyticsBuffer.length < 1) return Promise.resolve();

	let dateTime = new Date();
	let utcDateTime = new Date(dateTime.getUTCFullYear(), dateTime.getUTCMonth(), dateTime.getUTCDate(), dateTime.getUTCHours(), dateTime.getUTCMinutes(), dateTime.getUTCSeconds());
	let folderName = `${utcDateTime.getFullYear()}${utcDateTime.getMonth()}${utcDateTime.getDate()}`
	let filename = `ppa_${utcDateTime.toISOString().replace(/[:.\\-]/g, '_')}.json`

	return new Promise((resolve, reject) => {
		let analyticsString = '';
		while (analyticsBuffer.length > 0) {
			let analytic = analyticsBuffer.pop();
			analyticsString = analyticsString.concat(`\n${analytic.playerID},${analytic.levelID},${analytic.solution}`);
		}

		// Create temp file
		const tempFilePath = `temp.json`;
		fs.writeFileSync(tempFilePath, analyticsString);

		getFolderByName("puzzled-penguins-analytics")
			.then(folder => {
				const fileMetadata = {
					name: `${filename}`,
					parents: [folder.id]
				};

				const media = {
					mimeType: 'application/json',
					body: fs.createReadStream(tempFilePath)
				};

				console.log(`Uploading data to ${folder.name}/${fileMetadata.name}`);
				drive.files.create({
					auth: jwtClient,
					resource: fileMetadata,
					media,
					fields: 'id'
				}, (err, file) => {
					if (err) reject(err);
					else {
						console.log(`Uploaded data to ${folder.name}/${fileMetadata.name}. File ID: ${file}`);
						resolve(file);
					}
				});
			});
	});
}

function isNullOrWhitespace(input) { return !input || !input.trim(); }