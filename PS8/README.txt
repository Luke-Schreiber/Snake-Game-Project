Features
	
	1. Networking

		Players will be given a message saying they failed to connect to a server when this occurs.
		They can change their entered server address and try again.

		When a server closes with players connected, a message saying so will be displayed.
		In this case they can also try to connect once again without issue.

		When a player is connected to a server the connect button is greyed out, however, if they
		lost connection for any reason it will become usable again and they can try to connect to a server.
		
		There is an issue that can occur when someone joins one server, gets disconnected from it, then tries
		to join a different server, i.e they first join localhost and lost connection and then join 
		snake.eng.utah.edu. What happens is that their old snake from the first server will be visible to them in
		the second server. 

	2. View
		
		When a snake dies an explosion animation will play over its length.
		Snakes can "wrap" to the other side of the world by going off its edge and will be drawn correctly.
		There are 8 available snake colors, the first 8 players will all be different colors and then there will be repeats.
		At times, it seems if the server load is high, powerups will flicker a little.

		For our background, we created it using Dalle image generation by OpenAI. We entered in text prompts for space themed
		images to create our background image. Then we loaded in metal walls to fit our space theme.



Design Choices

	1.
		Within our World class we have a Dictionary titled framesSinceDeath which we use to draw the stages of
		the death animation that plays whenever a snake dies. The dictionary uses Snake IDs as the key and stores
		a count for the number of frames that have passed since its death(this count is inexact but functional for our purposes).
		We then use this counter to determine which stage of an explosion needs to be drawn for a snake, which in the
		end gives the effect of an explosion expanding and fading.
	2.
		In our view we have some "for" loops used for drawing that handle cases of different relative locations of the two
		points to be drawn between.
	3.
		We save snakes in the model stored in a Dictionary using their IDs as keys so that a specific snake can be easily
		found when needed.
	4.
		Our method which adds snakes to the world has alot of "if" statements in it, which we needed in order to make sure
		that a snake is never added to a Dictionary it currently exists in and would throw an exception. We also needed them
		for our death animation to work properly, as it seemed the best place to store and increment our counter which tells
		how long a snake has been dead for. One is also used in the logic to assign snakes different colors.
	5.
		We commented out the contents of the ContentPage_Focused method, as this method resulted in the breaking of our "about"
		and "help" buttons and did not seem to do anything else.

