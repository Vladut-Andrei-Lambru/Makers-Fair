# Maker’s Fair

A VR construction game set thirty years in the future, when practical making skills have fallen out of use. At a fair run by older makers, the player builds a cart from planks, nails and wheels, then tries to get it across a bridge without exceeding its weight limit.

[Gameplay](https://youtu.be/34psfsuSL3U) · [Construction demo](https://youtu.be/B_9jCtzeDWo) · [Case study](https://vladut-andrei-lambru.github.io/projects/makers-fair/)

## My contribution

I was the lead programmer in a five-person student team. I built the construction mechanics, guidance, level flow and UI, and contributed to level design. I did not create the art assets.

The main technical challenge was keeping connected planks stable while the player grabbed and moved them in VR. The implementation groups connected objects at runtime, merges groups as connections are made, and controls when objects follow a group and when they are fully simulated.

Other systems include hammer-hit validation, nail insertion, wheel attachment and a mass check for the bridge challenge.

## Design and testing

The first version offered little guidance so players could experiment freely. Testing showed that assembly needed clearer feedback. We added blueprints and holographic placement hints, then introduced the bridge weight limit to give the construction a concrete goal.

We planned a larger fair with several crafts. In the eight-week block, we completed one woodworking level.

## Run the project

- Unity **6000.2.2f1**, as recorded in `ProjectSettings/ProjectVersion.txt`.
- Open `My project/` through Unity Hub.
- Target hardware: Meta Quest 3. The VR interactions require a compatible headset and controllers.

Third-party assets retain their original licences.

[Portfolio](https://vladut-andrei-lambru.github.io/) · [v.lambru@st.hanze.nl](mailto:v.lambru@st.hanze.nl)
