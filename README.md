# Collection-Event-Rotation-Generator
Bloons TD 6 often has events in the game focused around collecting an in-game collectable called Insta Monkeys. There are 64 variants to collect of each monkey. 
The event has a "Featured Instas" section that allows you to guarantee you get a collectable of that specific monkey. This information is generated on the spot based on the event metadata inside the game instead of being listed somewhere directly.
This project allows you to generate the rotations accurately.

The sample code uses the data of the event from Ninja Kiwi's API, this is not a tutorial for how to get this info. 

A quick overview of how it works:

The rotation is generated using a seeded randomizer using the event's id. 

- First the event seed (Ex: `merp2vnt`) is converted into a numeric seed by using the ASCII character code of each symbol.
- Then that numeric seed is used to create a seeded random object, which will always return the same next random numbers in order.
- The seeded random object is used to shuffle the list of Insta Monkeys (specified by the featuredInstas list)
- Finally this list of randomized instas is converted into 'pages'.

The pages portion has some additional logic to prevent the same pages from happening at exactly the same time of day. See the code for how that happens.
It can also handle cases where not every tower is enabled (though this has never happened historically without Ninja Kiwi's explicit mention, such as removing Dart Monkey for one event when Mermonkey came out before they could fix it)

Originally based on an older implementation by [Minecool](https://github.com/Minecool), thanks for helping me figure out what changed to make it accurate again.
