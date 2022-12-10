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



	Server Design Choices

	1.
		We have a number of instance variables with are ints that we use to keep track of how many frames ago a snake died or powerup died,
		or how many frames ago a snake ate a powerup. These are used to do things like have a snake respawn after RespawnTime or the same
		for a powerup, and have a snake grow the set amount after eating.
	2.
		We have a lot of if statements in our Model that mostly handle a variety of cases for relative positions of objects.
	3.
		When choosing a respawn location for snakes, we generate a random number within Worldsize/2 and a buffer distance equal to 50(wall width)
		plus startingLength so that snakes cannot spawn with their head or their tail outside the world. We also check for collisions with walls.
	4.
		We handle quick 180 turns by making it so that the server will not process commands from the player that come in too soon after a turn such
		that it would cause them to turn into themselves.
	5.
		Our .xml has the extra optional settings mentioned in the assignment instructions under "Basic Data", but our code can also handle a .xml
		without these extras
	6.
		In our ServerController we have a disconnectedPlayers List that helps us manage when a player disconnects. We primarily chose to do this 
		so that we can remember which players have disconnected and can remove them on the frame after they have done so, as to avoid changing
		a list that is being iterated through currently.
	7.
		We made some of the instance variables of snakes and powerups public so that we could interact with them on the server side for things like
		collisions and the "died" state of a snake. We also added new public instance variables to record other things we needed to be able to manipulate
		from the model.
	8.
		There is a lot of arithmetic done in the model which mostly concerns finding the relative positions of a snake and various other things, whether
		they be powerups, walls, other snakes, or other sections of the snake itself.
	9.
		To prevent race conditions, we have locks in ServerController around things that manipulate the dictionary of clients and or the dictionary of snakes
		in serverWorld. We lock using "serverWorld".
	10.
		We check for the server settings file in the local directory, and then 3 up from that(where ServerController.cs is).


