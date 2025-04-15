import { useState } from 'react';

import Container from 'react-bootstrap/Container';

import Sheet1 from './components/sheet-1/Sheet1'
import Sheet2 from './components/sheet-2/Sheet2';

function App() {
  const [character, setCharacter] = useState({
    sheet1: {
      name: "character name",
      class: "none",
      level: 1,
      race: "character race",
      backstory: "character backstory",
      worldview: "character worldview",
      playerName: "player name",
      experience: 0,
      characteristics: {
        strength: {
          base: 15
        },
        dexterity: {
          base: 12
        },
        constitution: {
          base: 12
        },
        intelligence: {
          base: 14
        },
        wisdom: {
          base: 13
        },
        charisma: {
          base: 11
        }
      },
      isInspired: false,
      saveThrows: {
        strength: {
          isProficient: true
        },
        dexterity: {
          isProficient: true
        },
        constitution: {
          isProficient: false
        },
        intelligence: {
          isProficient: false
        },
        wisdom: {
          isProficient: false
        },
        charisma: {
          isProficient: false
        }
      },
      skills: {
        acrobatics: { isProficient: true, ability: "dexterity" },
        investigation: { isProficient: true, ability: "intelligence" },
        perception: { isProficient: false, ability: "wisdom" },
        survival: { isProficient: false, ability: "wisdom" },
        performance: { isProficient: false, ability: "charisma" },
        intimidation: { isProficient: false, ability: "charisma" },
        history: { isProficient: false, ability: "intelligence" },
        sleightOfHand: { isProficient: false, ability: "dexterity" },
        arcana: { isProficient: false, ability: "intelligence" },
        medicine: { isProficient: false, ability: "wisdom" },
        deception: { isProficient: false, ability: "charisma" },
        nature: { isProficient: false, ability: "intelligence" },
        insight: { isProficient: false, ability: "wisdom" },
        religion: { isProficient: false, ability: "intelligence" },
        stealth: { isProficient: false, ability: "dexterity" },
        persuasion: { isProficient: false, ability: "charisma" },
        animalHandling: { isProficient: false, ability: "wisdom" }
      },
      tools: [
        {
          id: 0,
          name: "Тяжёлые мечи",
          proficiencyType: "proficient", // proficient | expertise | jackOfAllTrades
          bondAbility: "strength",
          mods: 0
        },
        {
          id: 1,
          name: "tool2",
          proficiencyType: "proficient", // proficient | expertise | jackOfAllTrades
          bondAbility: "intelligence",
          mods: 0
        },
      ],
      otherTools: [
        {
          id: 0,
          name: "lang1",
          type: "language", // language, weapon, armor, other
        },
        {
          id: 1,
          name: "weapon1",
          type: "weapon", 
        },
        {
          id: 2,
          name: "armor1",
          type: "armor", 
        },
        {
          id: 3,
          name: "other1",
          type: "other", 
        },
      ],
      armorClass: 0,
      initiative: 0,
      speed: 0,
      hp: {
        max: 10,
        current: 10,
        temp: 5
      },
      hitDice: {
        total: 3,
        current: 3,
        type: "D8"
      },
      deathSaveThrow: {
        successes: [false, false, false],
        failures: [false, false, false]
      },
      personalityTraits: "character personality traits",
      ideals: "character ideals",
      bonds: "character bonds",
      flaws: "character flaws",
      classResource: {
        total: 2,
        current: 2,
        name: "Rage",
        usePb: false,
        resetOn: "longRest"
      },
      otherResources: [
        {
          id: 1,
          total: 2,
          current: 2,
          name: "other"
        }
      ]
    },
    sheet2: {
      age: "20 лет",
      height: "175 м",
      weight: "80 кг",
      eyes: "Зелёные",
      skin: "Светлая",
      hair: "Блондин",
      appearance: "character appearance",
      backstory: "character backstory",
      alliesAndOrganizations: "character alliesAndOrganizations",
      additionalFeaturesAndTraits: "additionalFeaturesAndTraits",
      treasures: "treasures"
    }
  });


  const [characterName, setCharacterName] = useState(character.sheet1.name);

  return (
    <Container>
      <Sheet1
        character={character}
        setCharacter={setCharacter}
        characterName={characterName}
        setCharacterName={setCharacterName}
      />

      {/*<Sheet2*/}
      {/*  character={character}*/}
      {/*  setCharacter={setCharacter}*/}
      {/*  characterName={characterName}*/}
      {/*  setCharacterName={setCharacterName}*/}
      {/*/>*/}
    </Container>
  );
}

export default App;