//
//  APDNativeAdQueue.h
//  Appodeal
//
//  AppodealSDK version 2.5.15
//
//  Copyright © 2020 Appodeal, Inc. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <Appodeal/APDNativeAd.h>
#import <Appodeal/APDDefines.h>
#import <Appodeal/APDNativeAdSettings.h>
#import <Appodeal/APDSdk.h>


@class APDNativeAdQueue;
/**
 Declaration of native ad queue delegate
 */
@protocol APDNativeAdQueueDelegate <NSObject>

@optional
/**
 Method called when loader receives native ad.
 @param adQueue ad queue object
 @param count count of available native ad
 */
- (void)adQueueAdIsAvailable:(nonnull APDNativeAdQueue *)adQueue ofCount:(NSUInteger)count;
/**
 Method called when loader fails to receive native ad.
 @param adQueue ad queue object
 @param error Error occurred
 */
- (void)adQueue:(nonnull APDNativeAdQueue *)adQueue failedWithError:(nonnull NSError *)error;

@end
/**
  Instance of APDNativeAdQueue
 */
@interface APDNativeAdQueue : NSObject
/**
 Set loader delegate
 */
@property (nonatomic, weak, nullable) id<APDNativeAdQueueDelegate> delegate;
/**
 Set queue settings
 */
@property (nonatomic, strong, nonnull) APDNativeAdSettings * settings;
/**
 Get count of available native ads
 */
@property (nonatomic, readonly, assign) NSInteger currentAdCount;
/**
 Set current placement
 */
@property (nonatomic, strong, nullable) NSString * placement;
/**
 Set autocache
 */
@property (nonatomic, assign) BOOL autocache;
/**
 Get precache ad count
 */
@property (nonatomic, readonly, assign) NSInteger precacheAdCount;
/**
 Get avaiable ads count for placement > 0
 */
@property (nonatomic, readonly, assign) BOOL containsSuitableAdsForCurrentPlacement;
/**
 Set custom sdk
 */
@property (weak, nonatomic, nullable) APDSdk *customSdk;
/**
 Initializator
 @param sdk Current sdk
 @param settings Queue settings
 @param delegate Queue delegate
 @param autocache Autocache
 @return Instance of APDNativeAdQueue
 */
+ (nonnull instancetype)nativeAdQueueWithSdk:(nullable APDSdk *)sdk
                                    settings:(nonnull APDNativeAdSettings *)settings
                                    delegate:(nullable id<APDNativeAdQueueDelegate>)delegate
                                   autocache:(BOOL)autocache;
/**
 Set max count native ad
 @param adSize max count of native ad
 */
- (void)setMaxAdSize:(NSInteger)adSize __attribute__((deprecated("Configure ad queue size in dashboard")));
/**
 Call this method to load native ad.
 */
- (void)loadAd;
/**
 Call this method to get native ads
 @param count available native ads count
 @return array of native ad
 */
- (nonnull NSArray <__kindof APDNativeAd *> *)getNativeAdsOfCount:(NSInteger)count;


@end
