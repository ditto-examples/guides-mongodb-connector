# Ditto - Sample MongoDB Connector `Solar System Guides` App

This is a sample application that demonstrates how to use the MongoDB Connector for Ditto with the **`guides`** sample data that comes with MongoDb Atlas. 

# Prerequisites

## MongoDb Atlas 

- Basic understanding of MongoDB Atlas
- Active MongoDB Atlas account with cluster pre-configured 
- [MongoDB Shell Installed](https://www.mongodb.com/docs/mongodb-shell/)

## SwiftUI 

- Basic understanding of Swift and SwiftUI for SwiftUI app
- Xcode 16 or higher

## Android with Jetpack Compose
- Basic understanding of Kotlin and Jetpack Compose for Android app
- Android Studio "Koala" 2024.1.1 or higher

## .NET Maui
- Basic understanding of .NET Maui and Shell for .NET Maui app
- Visual Studio 2022, Visual Studio Code with [.NET Maui extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-maui), Cursor with [.NET Maui extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-maui), or JetBrains Rider 2024.1 or higher

> [!NOTE]
> Ditto .NET SDK for Maui currently does NOT support Mac Catalyst or Windows, see [Ditto Docs - Compatibility Matrix](https://docs.ditto.live/sdk/latest/compatibility/c-sharp)
> 

# Setup in MongoDB Atlas

## Setup Atlas User from within MongoDB Atlas

For this setup, we will need to create two different users.  One for setting up the Cluster and one for the Ditto MongoDB Connector.

### Create a User for managing the Cluster

Open MongoDb Atlas and make sure you are on the Cluster Overview page.

1. Click on the Database Access link under the Security section from the navigation menu on the left.
2. Under the Database Users tab, click on the `+ ADD NEW DATABASE USER` button.
3. Make sure Authentication Method is set to `Password`. 
4. Under the first field, add in the username for the user.  In the examples we will use the username `atlasAdmin`.
5. Under the Password Authentication set a password for the user. 
6. Under Database User Privileges, click the `Add Built In Role` button and select the `Atlas admin` role.
7. Click the `Add User` button.

### Create a User for the Ditto MongoDB Connector

More information about why this user is needed can be found in the [Ditto documentation](https://docs.ditto.live/cloud/mongodb-connector#create-a-mongodb-database-user).

1. Under the `Database Users` tab, click on the `+ ADD NEW DATABASE USER` button.
2. Make sure Authentication Method is set to `Password`. 
3. Under the first field, add in the username for the user.  In the examples we will use the username `connector`.
4. Under the Password Authentication set a password for the user. 
5. Under `Database User Privileges`, click the `Add Built In Role` button and select the `Read and write to any database` role.
6. Click the `Add User` button.

## Setup Atlas Networking from within MongoDB Atlas

For this setup, we will need to add the IP Addresses for Ditto Big Peer to the list of allowed IP Addresses to communicate with the MongoDB Atlas cluster.  More information can be found in the [Ditto documentation](https://docs.ditto.live/cloud/mongodb-connector#add-ditto-ips-to-mongodb-allowlist).

### Add the IP Addresses for Ditto Big Peer to the list of allowed IP Addresses

Open MongoDb Atlas and make sure you are on the `Cluster Overview` page.

1. Click on the `Network Access` link under the `Security` section from the navigation menu on the left.  It should open to the `IP Access List` page.
2. Click on the `+ ADD IP ADDRESS` button.
3. From the Add IP Access List Entry window:
  - Add the following IP to the Access List Entry, enter:  **52.15.232.117/32**
  - For Comments add, enter:  **Ditto-52.15.232.117** 
4. Click the `Confirm` button.

Repeat this process for these IP Addresses:
  - **3.130.255.9/32**
  - **3.147.233.88/32**

## Load the Sample Data

This sample application uses the [sample_guides](https://www.mongodb.com/docs/atlas/sample-data/sample-guides/) database from the MongoDB Atlas cluster and the planets collection.  Use the [MongoDB Atlas documentation](https://www.mongodb.com/docs/guides/atlas/sample-data/) to load the sample_guides data into your cluster before moving forward.  More information on the sample_guides data can be found [here](https://www.mongodb.com/docs/atlas/sample-data/#std-label-load-sample-data).

This dataset was chosen for its small size, which eliminates the need to load large amounts of data into MongoDB Atlas and the Ditto Portal. This makes it an efficient way to test the Ditto MongoDB Connector.

## Setup Collection Settings with the MongoDB Shell

Ditto requires the planets collection to have change stream pre and post images enabled.  The following commands update the collection in order to enable [Change Stream Pre and Post Images](https://docs.ditto.live/cloud/mongodb-connector#create-mongodb-collections).  

### Getting the MongoDb Connection String

From Atlas, click on the Clusters link under the Database section from the menu on the left.  Next, click on the Connect button under your cluster listing.

From the Connect window, select `Shell` from the list.  This should give you the connection string for your cluster.

### Running the Commands in the MongoDB Shell

With the MongoDB Shell installed, run the following commands, replacing the srv:// with the proper connection string for your cluster and `atlasAdmin` with the username you created with admin rights to the cluster:

```sh
mongosh "mongodb+srv://freecluster.abcd1.mongodb.net/sample_guides" 
--apiVersion 1 --username atlasAdmin 
```

Once connected, run the following command to enable change stream pre and post images:

```sh
use sample_guides 
db.runCommand({ 
    collMod: "planets", 
    changeStreamPreAndPostImages: { enabled: true } 
})
```

Keep the MongoDB shell open as we will need it in future steps.

# Setup the Ditto MongoDB Connector

Currently, the Ditto MongoDB Connector will only work with documents that have been updated since the connector was created.  This means the workflow needs to be:

1. Create the Ditto MongoDB Connector in the Ditto Portal 
2. Use MongoDb Shell to update the planets collection with the `planetId`  and soft delete field `isArchived`
3. Validate in the Ditto Portal that the documents have been synced into Ditto with the `planetId` field


## Setup the Ditto MongoDB Connector in the Ditto Portal

The [Ditto documentation](https://docs.ditto.live/cloud/mongodb-connector#configuring-the-connector) has information about how to setup the Ditto MongoDB Connector in the Ditto Portal. 

The Step-By-Step Guide can be found [here](https://docs.ditto.live/cloud/mongodb-connector#step-by-step-guide)

## Updating the Planet with `planetId` and `isArchived` field

Go back to the MongoDb Shell.  Run the following command to add a unique planetId field to all documents in the planets collection and setting the soft delete field `isArchived` to false.  This should update all the documents in the collection, which should trigger the Ditto MongoDB Connector to sync the documents to Ditto:

```sh
db.planets.updateMany(
    { planetId: { $exists: false } },
    [
        { 
            $set: { 
                planetId: { $toString: "$_id" },
                isArchived: false 
            }
        }
    ]
)
```

This command will:
1. Find all documents that don't have a planetId field
2. Use the document's existing `_id` field (which is already a unique ObjectId)
3. Convert the ObjectId to a string
4. Add it as the planetId field
5. Add a `isArchived` field and set it to true for [soft delete](https://docs.ditto.live/crud/delete#Zb1T7) purposes with subscription 


You can verify the update by running:
```sh
db.planets.findOne()  
```
It should show a document with a planetId field, for example:

```json
{
  "_id": "621ff30d2a3e781873fcb65d",
  "_mdb": {
    "_id": "621ff30d2a3e781873fcb65d",
    "ct": [
      1737496378,
      185
    ],
    "tm": {
      "_id": 7
    }
  },
  "hasRings": true,
  "isArchived": false,
  "mainAtmosphere": [
    "H2",
    "He",
    "CH4"
  ],
  "name": "Uranus",
  "orderFromSun": 7,
  "planetId": "621ff30d2a3e781873fcb65d",
  "surfaceTemperatureC": {
    "max": null,
    "mean": -197.2,
    "min": null
  }
}
```

## Validate the documents have been updated with the planetId field

To validate that the documents are syncing in Ditto.

- Log into the [Ditto Portal](https://portal.ditto.live/).  
- Select your app.
- Click on the `Collections` tab
  - You should see the planets collection with the count of documents that were synced from MongoDb Atlas.  The count should be 8 documents.  
  - Click the `View` link for the planets collection to see the documents in the DQL Editor.


# Setup the SwiftUI App

## Setup the DittoConfig.plist file

Update the `dittoConfig.plist` file with the proper values for `appId` and `authToken`.  You can find these values in the Ditto Portal `Connect` tab listed as the App ID and Online Playground Authentication Token.

The endpointUrl in the config file is currently used to connect to the preview environment of Big Peer that is used for the Ditto MongoDB Connector.

## Run the SwiftUI App

Open the `Guides.xcodeproj` file in XCode that can be found in the Run the SwiftUI folder. 

# Setup the Android Jetpack Compose App

## Application Permissions for Ditto

The Ditto Android SDK requires specific system permissions to enable its core functionality, including peer-to-peer synchronization and network operations. These permissions are configured through:

1. Declaration in the `AndroidManifest.xml`
2. Runtime permission handling in `MainActivity.kt`

This sample application demonstrates the recommended permission implementation. However, developers should consult the [official Ditto documentation](https://docs.ditto.live/sdk/latest/install-guides/kotlin#declaring-permissions-in-the-android-manifest) for comprehensive guidance on permission requirements and best practices when integrating the SDK into their applications.

Required permissions include:
- Bluetooth permissions for peer-to-peer sync
- Location permissions for BLE scanning
- Network and WiFi permissions for cloud synchronization

For a complete list of permissions and implementation details, please refer to the documentation.

## Setup the dittoConfig.xml file

Update the `dittoConfig.xml` file found in the Android/app/src/main/res/values/ folder with the proper values for `appId` and `authToken`.  You can find these values in the Ditto Portal `Connect` tab listed as the App ID and Online Playground Authentication Token.

The `endpointUrl` in the config file is currently used to connect to the preview environment of Big Peer that is used for the Ditto MongoDB Connector.

## Run the Android Jetpack Compose App

Open the `Android` folder in Android Studio.  

> [!NOTE] 
> Make sure you open this folder specifically, otherwise Android Studio will not open the application properly because it will be missing the gradle files it needs to properly restore packages.     

# Known Limitations

An updated list of known limitations with the Ditto MongoDB Connector can be found [here](https://docs.ditto.live/cloud/mongodb-connector#current-limitations).

# YouTube Video
A YouTube video that walks you through the setup can be found [here](https://www.youtube.com/watch?v=BtssEpG4m38).
